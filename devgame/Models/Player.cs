using System;
using System.Collections.Generic;

namespace devgame.Models;

public partial class Player
{
    public decimal Id { get; set; }

    public string? Name { get; set; }

    public string? Difficulty { get; set; }

    public DateTime? Timestarted { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
