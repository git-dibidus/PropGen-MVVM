
namespace PropGen.WPF.Services
{
    public interface IAppDataService
    {
        void Save(ApplicationData data);
        ApplicationData Load();
    }
}
