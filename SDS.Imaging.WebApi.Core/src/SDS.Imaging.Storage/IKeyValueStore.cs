using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDS.Imaging.Storage
{
    public interface IKeyValueStore
    {
		void Save(string id, string value);
		void Save(string id, byte[] value);
		byte[] Load(string id);
    }
}
