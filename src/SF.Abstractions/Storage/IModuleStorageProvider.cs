namespace SFSharp.Abstractions.Storage;

public interface IModuleStorageProvider
{
    IModuleStorage GetAssets(ModuleDescriptor descriptor);
    IModuleStorage GetUserData(ModuleDescriptor descriptor);
    IModuleConfig GetConfig(ModuleDescriptor descriptor);
}
