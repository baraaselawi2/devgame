using System;
using System.Collections.Generic;

namespace devgame.Models;

public partial class Game
{
    public decimal Id { get; set; }

    public decimal? Playerid { get; set; }

    public DateTime? Timestarted { get; set; }

    public DateTime? Timeended { get; set; }

    public string? Currentscore { get; set; }

    public decimal? Totaltimespent { get; set; }

    public decimal? Bestquestionid { get; set; }

    public virtual Player? Player { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
