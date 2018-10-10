using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ravi.learn.idp.web.ViewModels
{
    public class OrderFrameViewModel
    {
        private readonly string _address;

        public string Address { get { return _address; } }

        public OrderFrameViewModel(string address)
        {
            _address = address;
        }
    }
}
