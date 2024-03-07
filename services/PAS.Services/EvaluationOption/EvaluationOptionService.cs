using PAS.Model.Domain;
using PAS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class EvaluationReviewOptionsService : IEvaluationReviewOptionsService
    {
        private readonly IEvaluationReviewOptionsRepository _evaluationReviewOptionsRepository;

        public EvaluationReviewOptionsService(IEvaluationReviewOptionsRepository evaluationReviewOptionsRepository)
        {
            _evaluationReviewOptionsRepository = evaluationReviewOptionsRepository;
        }

        public bool CreateEvluationReviewOption(IEnumerable<EvaluationReviewOptions> options)
        {
            options = options.Aggregate(new List<EvaluationReviewOptions>(), (newList, obj) =>
             {
                 if(obj.SortOrder == default(int))
                 {
                     obj.SortOrder = int.MaxValue;
                 }
                 newList.Add(obj);
                 return newList;
             });
            _evaluationReviewOptionsRepository.AddRange(options);
            return _evaluationReviewOptionsRepository.UnitOfWork.SaveEntities();
        }
        public IEnumerable<EvaluationReviewOptions> GetEvaluationReviewOptions()
        {
            return _evaluationReviewOptionsRepository.GetAll();
        }
        public bool UpdateEvaluationReviewOption(EvaluationReviewOptions option)
        {
            _evaluationReviewOptionsRepository.Update(option);
            return _evaluationReviewOptionsRepository.UnitOfWork.SaveEntities();
        }
    }
}
