namespace SoarCraft.QYun.ArknightsAssetStudio.Core.Contracts.Services {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    // Remove this class once your pages/features are using your data.
    public interface ISampleDataService {
        Task<IEnumerable<SampleOrder>> GetContentGridDataAsync();

        Task<IEnumerable<SampleOrder>> GetGridDataAsync();

        Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
    }
}
