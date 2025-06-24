using devgame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly ModelContext _db;

    public GameController(ModelContext db)
    {
        _db = db;
    }


    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartRequest request)
    {
        var player = new Player
        {
            Name = request.Name,
            Difficulty = request.Difficulty.ToString(),
            Timestarted = DateTime.Now
        };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        var game = new Game
        {
            Playerid = player.Id,
            Timestarted = DateTime.Now
        };
        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        var question = GenerateQuestion(request.Difficulty);
        question.Gameid = game.Id;
        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = $"Hello {request.Name}, find your submit API URL below",
            submit_url = $"/game/{game.Id}/submit",
            question = question.Questiontext,
            time_started = game.Timestarted
        });
    }

    [HttpPost("{gameId}/submit")]
    public async Task<IActionResult> SubmitAnswer(decimal gameId, [FromBody] SubmitRequest request)
    {
        var game = await _db.Games
            .Include(g => g.Questions)
            .Include(g => g.Player)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null || game.Timeended != null)
            return BadRequest("Invalid or ended game.");

        if (game.Player == null)
            return BadRequest("Player not found for this game.");

        var lastQuestion = game.Questions
            .OrderByDescending(q => q.Id)
            .FirstOrDefault();

        if (lastQuestion == null || lastQuestion.Submittedanswer != null)
            return BadRequest("No active question to answer.");

        var submittedTime = DateTime.Now;
        var timeTaken = (decimal)(submittedTime - (lastQuestion.Timesubmitted ?? game.Timestarted)).Value.TotalSeconds;

        lastQuestion.Submittedanswer = request.Answer.ToString();
        lastQuestion.Timesubmitted = submittedTime;
        lastQuestion.Timetaken = timeTaken;

        double actualAnswer;
        if (!double.TryParse(lastQuestion.Answer, out actualAnswer))
            return BadRequest("Stored answer is not a valid number.");

        lastQuestion.Iscorrect = (Math.Abs(actualAnswer - request.Answer) < 0.001) ? "true" : "false";

        int difficulty = 1;
        int.TryParse(game.Player.Difficulty, out difficulty);
        var newQuestion = GenerateQuestion(difficulty);
        newQuestion.Gameid = game.Id;
        _db.Questions.Add(newQuestion);

        var correct = game.Questions.Count(q => q.Iscorrect == "true");
        var total = game.Questions.Count(q => q.Submittedanswer != null);
        game.Currentscore = $"{correct}/{total}";

        await _db.SaveChangesAsync();

        return Ok(new
        {
            result = lastQuestion.Iscorrect == "true"
                ? $"Good job {game.Player.Name}, your answer is correct!"
                : $"Sorry {game.Player.Name}, your answer is incorrect.",
            time_taken = timeTaken,
            next_question = new
            {
                submit_url = $"/game/{game.Id}/submit",
                question = newQuestion.Questiontext
            },
            current_score = game.Currentscore
        });
    }

    [HttpGet("{gameId}/end")]
    public async Task<IActionResult> EndGame(decimal gameId)
    {
        var game = await _db.Games
            .Include(g => g.Player)
            .Include(g => g.Questions)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            return NotFound();

        game.Timeended = DateTime.Now;
        game.Totaltimespent = game.Questions.Sum(q => q.Timetaken ?? 0);
        game.Currentscore = $"{game.Questions.Count(q => q.Iscorrect == "true")}/{game.Questions.Count()}";

        var best = game.Questions
            .Where(q => q.Iscorrect == "true" && q.Timetaken != null)
            .OrderBy(q => q.Timetaken)
            .FirstOrDefault();

        if (best != null)
            game.Bestquestionid = best.Id;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            name = game.Player.Name,
            difficulty = game.Player.Difficulty,
            current_score = game.Currentscore,
            total_time_spent = game.Totaltimespent,
            best_score = best == null ? null : new
            {
                question = best.Questiontext,
                answer = best.Answer,
                time_taken = best.Timetaken
            },
            history = game.Questions.Select(q => new
            {
                question = q.Questiontext,
                answer = q.Submittedanswer,
                time_taken = q.Timetaken
            })
        });
    }

    
    private Question GenerateQuestion(int difficulty)
    {
        var rnd = new Random();
        int operands = difficulty + 1;
        int digitLength = difficulty;

        List<string> numbers = new();
        List<string> ops = new() { "+", "-", "*", "/" };

        for (int i = 0; i < operands; i++)
        {
            int max = (int)Math.Pow(10, digitLength);
            int num = rnd.Next(1, max);
            numbers.Add(num.ToString());
        }

        string expression = numbers[0];
        for (int i = 1; i < operands; i++)
        {
            string op = ops[rnd.Next(ops.Count)];
            expression += $" {op} {numbers[i]}";
        }

        var result = new System.Data.DataTable().Compute(expression, "");

        return new Question
        {
            Questiontext = expression,
            Answer = result.ToString()
        };
    }
}
