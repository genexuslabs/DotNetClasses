// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "<Pending>", Scope = "member", Target = "~M:GeneXus.Data.Cosmos.CosmosDBDataReader.GetPage~System.Threading.Tasks.Task{System.Boolean}")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:GeneXus.Data.Cosmos.CosmosDBDataReader.Read~System.Boolean")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:GeneXus.Data.NTier.CosmosDB.RequestWrapper.Read~GeneXus.Data.NTier.CosmosDB.ResponseWrapper")]
[assembly: SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "<Pending>", Scope = "member", Target = "~M:GeneXus.Data.NTier.CosmosDB.RequestWrapper.ReadItemAsyncByPK(System.String,System.Object)~System.Threading.Tasks.Task{GeneXus.Data.NTier.CosmosDB.ResponseWrapper}")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:GeneXus.Data.NTier.CosmosDBConnection.ExecuteNonQuery(GeneXus.Data.NTier.ServiceCursorDef,System.Data.IDataParameterCollection,System.Data.CommandBehavior)~System.Int32")]
