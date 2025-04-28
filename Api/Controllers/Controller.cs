using Domain.Database;
using Domain.Services;
using Domain.Types;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class QueryController : ControllerBase
{
    [HttpGet]
    public IActionResult Query([FromQuery] string query)
    {
        var result = ParserService.Parse(query);

        if (result.IsFailure)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        var parsedQuery = result.Value!;

        var fromNode = new Node($"{Keywords.FROM} {parsedQuery.FromClause}");

        var joinNodes = parsedQuery
            .Joins.Select(join => new Node(
                $"{Keywords.JOIN} {join.Table} {Keywords.ON} {join.Condition}"
            ))
            .ToList();

        foreach (var joinNode in joinNodes)
        {
            fromNode.Children.Add(joinNode);
        }

        Node? whereNode = null;

        if (!string.IsNullOrEmpty(parsedQuery.WhereClause))
        {
            whereNode = new Node($"{Keywords.WHERE} {parsedQuery.WhereClause}");
            fromNode.Children.Add(whereNode);

            foreach (var joinNode in joinNodes)
            {
                joinNode.Children.Add(whereNode);
            }
        }

        var selectNode = new Node($"{Keywords.SELECT} {parsedQuery.SelectClause}");

        if (whereNode != null)
        {
            whereNode.Children.Add(selectNode);
        }
        else
        {
            fromNode.Children.Add(selectNode);

            foreach (var joinNode in joinNodes)
            {
                joinNode.Children.Add(selectNode);
            }
        }

        return Ok(new { query = parsedQuery, graph = fromNode });
    }
}
