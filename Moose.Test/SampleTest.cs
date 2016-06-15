using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Moose.Test
{
    // see example explanation on xUnit.net website:
    // https://xunit.github.io/docs/getting-started-dnx.html
    public class SampleTest
    {
        [Fact]
        public void ICanSeeData()
        {
            var conn = new Moose.Connection();
            conn.Execute();
        }

    }
}
