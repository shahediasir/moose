using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Moose.Facts
{
    public class Facts
    {
        [Fact]
        public void do_something()
        {
            Helpers.ResetDb("inheritance");
        }
    }
}
