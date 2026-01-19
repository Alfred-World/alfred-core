using Alfred.Core.Application.Querying.Ast;

namespace Alfred.Core.Application.Querying.Parsing;

/// <summary>
/// Interface cho filter parser
/// </summary>
public interface IFilterParser
{
    FilterNode Parse(string filter);
}
