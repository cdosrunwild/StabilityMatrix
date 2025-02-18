﻿using NLog;
using Refit;
using StabilityMatrix.Core.Api;
using StabilityMatrix.Core.Database;
using StabilityMatrix.Core.Models.Api;

namespace StabilityMatrix.Core.Helper;

// return Model, ModelVersion, ModelFile
public record struct ModelSearchResult(CivitModel Model, CivitModelVersion ModelVersion, CivitFile ModelFile);

public class ModelFinder
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ILiteDbContext liteDbContext;
    private readonly ICivitApi civitApi;
    
    public ModelFinder(ILiteDbContext liteDbContext, ICivitApi civitApi)
    {
        this.liteDbContext = liteDbContext;
        this.civitApi = civitApi;
    }

    // Finds a model from the local database using file hash
    public async Task<ModelSearchResult?> LocalFindModel(string hashBlake3)
    {
        var (model, version) = await liteDbContext.FindCivitModelFromFileHashAsync(hashBlake3);
        if (model == null || version == null)
        {
            return null;
        }

        var file = version.Files!
            .First(file => file.Hashes.BLAKE3?.ToLowerInvariant() == hashBlake3);
        
        return new ModelSearchResult(model, version, file);
    }
    
    // Finds a model using Civit API using file hash
    public async Task<ModelSearchResult?> RemoteFindModel(string hashBlake3)
    {
        Logger.Info("Searching Civit API for model version using hash {Hash}", hashBlake3);
        try
        {
            var versionResponse = await civitApi.GetModelVersionByHash(hashBlake3);
            
            Logger.Info("Found version {VersionId} with model id {ModelId}", 
                versionResponse.Id, versionResponse.ModelId);
            var model = await civitApi.GetModelById(versionResponse.ModelId);
            
            // VersionResponse is not actually the full data of ModelVersion, so find it again
            var version = model.ModelVersions!.First(version => version.Id == versionResponse.Id);
        
            var file = versionResponse.Files
                .First(file => file.Hashes.BLAKE3?.ToLowerInvariant() == hashBlake3);
        
            return new ModelSearchResult(model, version, file);
        }
        catch (ApiException e)
        {
            Logger.Info("Could not find remote model version using hash {Hash}: {Error}", 
                hashBlake3, e.Message);
            return null;
        }
    }
}
