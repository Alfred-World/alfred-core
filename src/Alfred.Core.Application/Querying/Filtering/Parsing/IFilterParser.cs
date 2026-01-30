using Alfred.Core.Application.Querying.Filtering.Ast;

namespace Alfred.Core.Application.Querying.Filtering.Parsing;

/// <summary>
/// Interface cho filter parser
/// </summary>
public interface IFilterParser
{
    FilterNode Parse(string filter);
}
