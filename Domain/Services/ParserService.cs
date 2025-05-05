using System.Text.RegularExpressions;
using Domain.Database;
using Domain.Regexes;
using Domain.Types;

namespace Domain.Services;

public static class ParserService
{
    public static Result<string> Parse(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return Result<string>.Failure("Query is empty.");

        var parseResult = ParseQuery(sql);

        if (parseResult.IsFailure)
            return Result<string>.Failure(parseResult.ErrorMessage);

        var query = parseResult.Value!;

        var validationResult = ValidateQuery(query);

        if (validationResult.IsFailure)
            return Result<string>.Failure(validationResult.ErrorMessage);

        var algebra = GenerateRelationalAlgebra(query);

        return Result<string>.Success(algebra);
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
            : null;

        parsed.FromAlias =
            !string.IsNullOrEmpty(aliasCandidate) && !Keywords.IsReserved(aliasCandidate)
                ? aliasCandidate
                : parsed.FromClause;

        parsed.TableAliases[parsed.FromAlias] = parsed.FromClause;

        var joinMatches = RootRegex.JoinWithAliasRegex().Matches(sql);
        foreach (var match in joinMatches.Cast<Match>())
        {
            var table = match.Groups[1].Value.Trim();
            var aliasCandidateJoin = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;
            var alias =
                !string.IsNullOrEmpty(aliasCandidateJoin)
                && !Keywords.IsReserved(aliasCandidateJoin)
                    ? aliasCandidateJoin
                    : table;

            var condition = match.Groups[3].Value.Trim();
            parsed.Joins.Add((table, alias, condition));
            parsed.TableAliases[alias] = table;
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

        return Result<string>.Success();
    }

    private static bool FieldExists(string field, Dictionary<string, string> aliases)
    {
        if (field.Contains('.'))
        {
            var parts = field.Split('.');
            if (parts.Length != 2)
                return false;

            var prefix = parts[0];
            var column = parts[1];

            if (aliases.TryGetValue(prefix, out var table))
                return Schema.Columns.TryGetValue(table, out var columns)
                    && columns.Contains(column);

            if (Schema.Tables.Contains(prefix))
                return Schema.Columns.TryGetValue(prefix, out var columns)
                    && columns.Contains(column);

            return false;
        }

        var possibleMatches = aliases
            .Values.Distinct()
            .Where(table =>
                Schema.Columns.TryGetValue(table, out var columns) && columns.Contains(field)
            )
            .ToList();

        return possibleMatches.Count == 1;
    }

    private static string GenerateRelationalAlgebra(ParsedQuery query)
    {
        var proj = query.SelectClause == Keywords.ASTERISK ? "π_*" : $"π_{query.SelectClause}";

        var useAlias = query.FromAlias != query.FromClause;
        var rel = useAlias ? $"{query.FromAlias} ← {query.FromClause}" : query.FromClause;

        foreach (var (table, alias, cond) in query.Joins)
        {
            rel = $"({rel} ⨝_{cond} {(alias != table ? $"{alias} ← {table}" : table)})";
        }

        var where = string.IsNullOrWhiteSpace(query.WhereClause)
            ? null
            : $"σ_{TransformOperators(query.WhereClause)}";

        return where is not null ? $"{proj}({where}({rel}))" : $"{proj}({rel})";
    }

    private static string TransformOperators(string cond)
    {
        cond = cond.Replace($" {Keywords.AND} ", " ∧ ").Replace($" {Keywords.OR} ", " ∨ ");

        var inMatch = RootRegex.InWithSubqueryRegex().Match(cond);

        if (!inMatch.Success)
            return cond;

        var leftField = inMatch.Groups[1].Value.Trim();
        var subquery = inMatch.Groups[2].Value.Trim();

        var subParsed = Parse(subquery);
        if (subParsed.IsFailure)
            return cond;

        var subMatch = RootRegex.SelectFieldFromTableRegex().Match(subquery);

        if (!subMatch.Success)
            return subParsed.Value!;

        var rightField = subMatch.Groups[1].Value.Trim();
        var subAlgebra = subParsed.Value!;

        return $"{leftField} = {rightField} ⨝ ({subAlgebra})";
    }
}
