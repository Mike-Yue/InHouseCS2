using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InHouseCS2.Core.Managers.Contracts.Models;

public record PlayerMapAggregatedData
{
    public required string Map { get; init; }
    public required double Kills { get; init; }
    public required double Assists { get; init; }
    public required double Deaths { get; init; }
    public required double KD { get; init; }
    public required double KAD { get; init; }
    public required decimal HSP { get; init; }
    public required double HEDamage { get; init; }
    public required double BurnDamage { get; init; }
    public required double EnemiesFlashed { get; init; }
    public required int GamesWon { get; init; }
    public required int GamesTied { get; init; }
    public required int GamesLost { get; init; }
    public required double WinPercentage { get; init; }
}
