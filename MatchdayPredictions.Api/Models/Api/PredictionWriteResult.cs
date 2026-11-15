namespace MatchdayPredictions.Api.Models.Api;

public enum PredictionWriteResult
{
    Created,
    Updated,
    MatchNotFound,
    MatchStarted,
    NotLeagueMember
}
