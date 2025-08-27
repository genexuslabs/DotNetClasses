using System;
using System.Threading.Tasks;
using GxClasses.Helpers;
namespace GeneXus.Utils
{
	public class GxEmbedding
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxEmbedding>();


		ReadOnlyMemory<float> _embedding;
		public GxEmbedding()
		{
		}
		public GxEmbedding(string model, int dimensions)
		{
			Model = model;
			Dimensions = dimensions;
			_embedding = new ReadOnlyMemory<float>(new float[dimensions]);
		}
		internal GxEmbedding(ReadOnlyMemory<float> embedding, string model, int dimensions) : this(model, dimensions)
		{
			_embedding = embedding;
		}
		internal GxEmbedding(float[] embedding)
		{
			_embedding = embedding;
		}

		internal static GxEmbedding Empty(string model, int dimensions)
		{
			return new GxEmbedding(model, dimensions);
		}
		public override string ToString()
		{
			return $"[{string.Join(",", _embedding.Span.ToArray())}]";
		}
		public static GxEmbedding GenerateEmbedding(GxEmbedding embeddingInfo, string text, GXBaseCollection<SdtMessages_Message> Messages)
		{
			try
			{
				ReadOnlyMemory<float> embedding = AIEmbeddingFactory.Instance.GenerateEmbeddingAsync(embeddingInfo.Model, embeddingInfo.Dimensions, text).GetAwaiter().GetResult();
				return new GxEmbedding(embedding, embeddingInfo.Model, embeddingInfo.Dimensions);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, ex, "Error generating embedding. Model: ", embeddingInfo?.Model, " Dimensions: ", embeddingInfo?.Dimensions.ToString(), " Text: ", text);
				GXUtil.ErrorToMessages("GenerateEmbedding Error", ex, Messages, false);
				return embeddingInfo;
			}
		}
		public string Model { get; set; }
		public int Dimensions { get; set; }

		internal ReadOnlyMemory<float> Data => _embedding;
	}
	internal interface IEmbeddingService
	{
		Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string model, int dimensions, string input);
	}
	internal class AIEmbeddingFactory
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<AIEmbeddingFactory>();

		private static volatile IEmbeddingService instance;
		private static object syncRoot = new object();
		private const string AI_PROVIDER = "GeneXus.AI.EmbeddingService, GxAI, Version=10.1.0.0, Culture=neutral, PublicKeyToken=null";
		public static IEmbeddingService Instance
		{
			get
			{
				if (instance == null)
				{
					lock (syncRoot)
					{
						if (instance == null)
						{

							try
							{
								Type type = AssemblyLoader.GetType(AI_PROVIDER);
								instance = (IEmbeddingService)Activator.CreateInstance(type);
							}
							catch (Exception e)
							{
								GXLogging.Error(log, "Couldn't create AI Provider", e);
								throw e;
							}
						}
					}
				}
				return instance;
			}
		}
	}
}
