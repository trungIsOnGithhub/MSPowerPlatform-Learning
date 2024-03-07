using PAS.Model.Domain;
using System.Collections.Generic;

namespace PAS.Services
{
    public interface IEvaluationReviewOptionsService
    {
        bool CreateEvluationReviewOption(IEnumerable<EvaluationReviewOptions> options);
        IEnumerable<EvaluationReviewOptions> GetEvaluationReviewOptions();
        bool UpdateEvaluationReviewOption(EvaluationReviewOptions option);
    }
}