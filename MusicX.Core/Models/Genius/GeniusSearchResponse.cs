using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models.Genius;

public record GeniusSearchResponse(
IReadOnlyList<Hit> Hits
);