using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mantensei_Database.DataAccess
{
    internal interface ISaveDataProvider
    {
        void LoadItem(IEnumerable<string> items);
        IEnumerable<string> GetSaveItems();
    }

    public static class SaveDataProviderExtensions
    {

    }
}
