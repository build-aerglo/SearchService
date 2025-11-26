// using Elastic.Clients.Elasticsearch.Analysis;
// using Elastic.Clients.Elasticsearch.IndexManagement;
// using Elastic.Clients.Elasticsearch.Mapping;
// using SearchService.Application.DTOs;
// using SearchService.Infrastructure.Constants;
//
// namespace SearchService.Infrastructure.Elasticsearch.Mappings;
//
// public class BusinessIndexMapping : IIndexMapping<BusinessIndexDto>
// {
//     public CreateIndexRequest ConfigureIndex(string indexName)
//     {
//         return new CreateIndexRequest(indexName)
//         {
//             Mappings = new TypeMapping
//             {
//                 Properties = new Properties
//                 {
//                     { "businessId", new KeywordProperty() },
//                     { "name", new TextProperty { Analyzer = "standard", Fields = new Properties { { "keyword", new KeywordProperty() } } } },
//                     { "website", new KeywordProperty() },
//                     { "isBranch", new BooleanProperty() },
//                     { "avgRating", new FloatNumberProperty() },
//                     { "reviewCount", new IntegerNumberProperty() },
//                     { "parentBusinessId", new KeywordProperty() },
//                     { 
//                         "categories", new NestedProperty
//                         {
//                             Properties = new Properties
//                             {
//                                 { "id", new KeywordProperty() },
//                                 { "name", new TextProperty { Analyzer = "standard" } },
//                                 { "description", new TextProperty() },
//                                 { "parentCategoryId", new KeywordProperty() }
//                             }
//                         }
//                     },
//                     { "createdAt", new DateProperty() },
//                     { "updatedAt", new DateProperty() }
//                 }
//             },
//             Settings = new IndexSettings
//             {
//                 NumberOfShards = 1,
//                 NumberOfReplicas = 1,
//                 // Analysis = new Analysis
//                 // {
//                 //     Analyzers = new Analyzers
//                 //     {
//                 //         { "custom_analyzer", new CustomAnalyzer { Tokenizer = "standard", Filter = new[] { "lowercase", "ascii_folding" } } }
//                 //     }
//                 // }
//             }
//         };
//     }
// }

using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using SearchService.Application.DTOs;

namespace SearchService.Infrastructure.Elasticsearch.Mappings;

public class BusinessIndexMapping : IIndexMapping<BusinessIndexDto>
{
    public CreateIndexRequest ConfigureIndex(string indexName)
    {
        return new CreateIndexRequest(indexName)
        {
            Mappings = new TypeMapping
            {
                Dynamic = DynamicMapping.True,
                Properties = new Properties
                {
                    { "businessId", new KeywordProperty() },
                }
            },
            Settings = new IndexSettings
            {
                NumberOfShards = 1,
                NumberOfReplicas = 0  // Set to 0 for development
            }
        };
    }
}