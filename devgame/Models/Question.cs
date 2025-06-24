using System;
using System.Collections.Generic;

namespace devgame.Models;

public partial class Question
{
    public decimal Id { get; set; }

    public decimal? Gameid { get; set; }

    public string? Questiontext { get; set; }

    public string? Answer { get; set; }

    public string? Submittedanswer { get; set; }

    public string? Iscorrect { get; set; }

    public DateTime? Timesubmitted { get; set; }

    public decimal? Timetaken { get; set; }

    public virtual Game? Game { get; set; }
}
