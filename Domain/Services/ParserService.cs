using System.Text.RegularExpressions;
using Domain.Database;
using Domain.Regexes;
using Domain.Types;

namespace Domain.Services;

public static class ParserService
{
    public static Result<object> Parse(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return Result<object>.Failure("Query is empty.");

        var parseResult = ParseQuery(sql);
        if (parseResult.IsFailure)
            return Result<object>.Failure(parseResult.ErrorMessage);

        var query = parseResult.Value!;
        var validationResult = ValidateQuery(query);
        if (validationResult.IsFailure)
            return Result<object>.Failure(validationResult.ErrorMessage);

        var algebraString = GenerateRelationalAlgebra(query);
        var algebraGraph = GenerateAlgebraGraph(query);
        var algebraDto = ConvertToDto(algebraGraph);

        return algebraDto.IsFailure
            ? Result<object>.Failure(algebraDto.ErrorMessage)
            : Result<object>.Success(new { query = algebraString, graph = algebraDto.Value });
    }

    private static ProjectionNode GenerateAlgebraGraph(ParsedQuery query)
    {
        var selectionsPerTable = new Dictionary<string, List<string>>();

        if (!string.IsNullOrWhiteSpace(query.WhereClause))
        {
            var conditions = query.WhereClause.Split(
                [Keywords.AND],
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var condRaw in conditions)
            {
                var cond = condRaw.Trim();

                var op = Keywords
                    .Operators.OrderByDescending(o => o.Length)
                    .FirstOrDefault(o => cond.Contains($" {o} "));

                if (op == null || !cond.Contains('.'))
                    continue;

                var parts = cond.Split(op, 2);
                if (parts.Length != 2)
                    continue;

                var table = parts[0].Split('.')[0].Trim();
                if (!selectionsPerTable.ContainsKey(table))
                    selectionsPerTable[table] = [];

                selectionsPerTable[table].Add(cond);
            }
        }

        var tableNodes = new Dictionary<string, AlgebraNode>();
        var fromTable = query.FromClause;
        tableNodes[fromTable] = WrapWithSelection(fromTable, selectionsPerTable);

        foreach (var (table, _, _) in query.Joins)
        {
            tableNodes[table] = WrapWithSelection(table, selectionsPerTable);
        }

        var joinTree = tableNodes[fromTable];
        foreach (var (table, _, condition) in query.Joins)
        {
            var rightNode = tableNodes[table];
            joinTree = new JoinNode(condition, joinTree, rightNode);
        }

        return new ProjectionNode(query.SelectClause, joinTree);
    }

    private static AlgebraNode WrapWithSelection(
        string table,
        Dictionary<string, List<string>> selectionsPerTable
    )
    {
        AlgebraNode node = new TableNode(table);

        if (!selectionsPerTable.TryGetValue(table, out var conditions))
            return node;

        var cond = string.Join(" ∧ ", conditions);
        node = new SelectionNode(cond, node);
        return node;
    }

    private static Result<AlgebraGraphDto> ConvertToDto(AlgebraNode node)
    {
        return node switch
        {
            ProjectionNode p => Result<AlgebraGraphDto>.Success(
                new AlgebraGraphDto
                {
                    Type = "Projection",
                    Label = $"π_{p.Attributes}",
                    Children = [ConvertToDto(p.Child).Value!],
                }
            ),
            SelectionNode s => Result<AlgebraGraphDto>.Success(
                new AlgebraGraphDto
                {
                    Type = "Selection",
                    Label = $"σ_{s.Condition}",
                    Children = [ConvertToDto(s.Child).Value!],
                }
            ),
            JoinNode j => Result<AlgebraGraphDto>.Success(
                new AlgebraGraphDto
                {
                    Type = "Join",
                    Label = $"⨝_{j.Condition}",
                    Children = [ConvertToDto(j.Left).Value!, ConvertToDto(j.Right).Value!],
                }
            ),
            TableNode t => Result<AlgebraGraphDto>.Success(
                new AlgebraGraphDto
                {
                    Type = "Table",
                    Label = t.TableName,
                    Children = [],
                }
            ),
            _ => Result<AlgebraGraphDto>.Failure("Unknown node type"),
        };
    }

    private static Result<ParsedQuery> ParseQuery(string sql)
    {
        var parsed = new ParsedQuery();

        var selectMatch = RootRegex.SelectRegex().Match(sql);
        if (!selectMatch.Success)
            return Result<ParsedQuery>.Failure(
                "Malformed query: missing or invalid SELECT clause."
            );

        var fromMatch = RootRegex.FromWithAliasRegex().Match(sql);
        if (!fromMatch.Success)
            return Result<ParsedQuery>.Failure("Malformed query: missing or invalid FROM clause.");

        parsed.SelectClause = selectMatch.Groups[1].Value.Trim();
        parsed.FromClause = fromMatch.Groups[1].Value.Trim();

        var aliasCandidate =
            fromMatch.Groups[2].Success ? fromMatch.Groups[2].Value.Trim()
            : fromMatch.Groups[3].Success ? fromMatch.Groups[3].Value.Trim()
            : parsed.FromClause;

        parsed.FromAlias = aliasCandidate;
        parsed.TableAliases[aliasCandidate] = parsed.FromClause;

        var joinMatches = RootRegex.JoinWithAliasRegex().Matches(sql);
        foreach (var match in joinMatches.Cast<Match>())
        {
            var table = match.Groups[1].Value.Trim();
            var aliasJoin = match.Groups[2].Success ? match.Groups[2].Value.Trim() : table;
            var condition = match.Groups[3].Value.Trim();

            parsed.Joins.Add((table, aliasJoin, condition));
            parsed.TableAliases[aliasJoin] = table;
        }

        var whereMatch = RootRegex.WhereRegex().Match(sql);
        if (whereMatch.Success)
            parsed.WhereClause = whereMatch.Groups[1].Value.Trim();

        return Result<ParsedQuery>.Success(parsed);
    }

    private static Result<string> ValidateQuery(ParsedQuery query)
    {
        foreach (var table in query.TableAliases.Values.Distinct())
        {
            if (!Schema.Tables.Contains(table))
                return Result<string>.Failure($"Table '{table}' does not exist.");
        }

        var fields = query.SelectClause.Split(',').Select(f => f.Trim()).ToList();
        if (fields is [Keywords.ASTERISK])
            return Result<string>.Success();

        foreach (var field in fields.Where(field => !FieldExists(field, query.TableAliases)))
        {
            return Result<string>.Failure($"Field '{field}' does not exist.");
        }

        if (string.IsNullOrWhiteSpace(query.WhereClause))
            return Result<string>.Success();
        {
            var conditions = query.WhereClause.Split(
                [Keywords.AND],
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var condRaw in conditions)
            {
                var cond = condRaw.Trim();

                var op = Keywords
                    .Operators.OrderByDescending(o => o.Length)
                    .FirstOrDefault(o => cond.Contains($" {o} "));

                if (op == null || !cond.Contains('.'))
                    continue;

                var parts = cond.Split(op, 2);
                if (parts.Length != 2)
                    continue;

                var field = parts[0].Trim();
                var value = parts[1].Trim().Trim('\'');

                if (!FieldExists(field, query.TableAliases))
                    return Result<string>.Failure($"Field '{field}' in WHERE does not exist.");

                var partsField = field.Split('.');
                var alias = partsField[0];
                var column = partsField[1];

                if (!query.TableAliases.TryGetValue(alias, out var table))
                    return Result<string>.Failure($"Alias '{alias}' not found.");

                if (
                    !Schema.ColumnTypes.TryGetValue(table, out var types)
                    || !types.TryGetValue(column, out var expectedType)
                )
                    return Result<string>.Failure($"Could not determine type for '{field}'.");

                var typeResult = ValidateType(expectedType, value);
                if (typeResult.IsFailure)
                    return Result<string>.Failure(
                        $"Invalid type for '{field}': {typeResult.ErrorMessage}"
                    );
            }
        }

        return Result<string>.Success();
    }

    private static Result<bool> ValidateType(string expectedType, string value)
    {
        return expectedType.ToLowerInvariant() switch
        {
            "int" => int.TryParse(value, out _)
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Expected integer, but got '{value}'."),

            "decimal" => decimal.TryParse(value, out _)
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Expected decimal, but got '{value}'."),

            "string" => int.TryParse(value, out _) || decimal.TryParse(value, out _)
                ? Result<bool>.Failure($"Expected string, but got numeric literal '{value}'.")
                : Result<bool>.Success(true),

            "datetime" => DateTime.TryParse(value, out _)
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Expected datetime, but got '{value}'."),

            _ => Result<bool>.Failure($"Unsupported type '{expectedType}'."),
        };
    }

    private static bool FieldExists(string field, Dictionary<string, string> aliases)
    {
        if (field.Contains('.'))
        {
            var parts = field.Split('.');
            if (parts.Length != 2)
                return false;

            var alias = parts[0];
            var column = parts[1];

            if (!aliases.TryGetValue(alias, out var table))
                return false;

            return Schema.Columns.TryGetValue(table, out var columns) && columns.Contains(column);
        }

        var possibleMatches = aliases
            .Values.Where(table =>
                Schema.Columns.TryGetValue(table, out var cols) && cols.Contains(field)
            )
            .ToList();

        return possibleMatches.Count == 1;
    }

    private static string GenerateRelationalAlgebra(ParsedQuery query)
    {
        var proj = $"π_{query.SelectClause}";
        var rel = query.FromClause;

        foreach (var (table, _, cond) in query.Joins)
        {
            rel = $"({rel} ⨝_{cond} {table})";
        }

        if (string.IsNullOrWhiteSpace(query.WhereClause))
            return $"{proj}({rel})";

        var where = $"σ_{query.WhereClause}";
        return $"{proj}({where}({rel}))";
    }
}
