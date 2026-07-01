using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relay.Intranet.Domain.Aggregates
{
    public sealed record Countries(
    string? code,
    string? name);
}
