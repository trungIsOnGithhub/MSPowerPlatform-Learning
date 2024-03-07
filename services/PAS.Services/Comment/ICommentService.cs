using PAS.Model.Dto;
using System.Collections;
using System.Collections.Generic;

namespace PAS.Services.Comment
{
    public interface ICommentService
    {
        bool CreateComment(Model.Comment comment);
        bool DeleteComment(int commentId);
        bool UpdateComment(Model.Comment comment);
        IEnumerable<Model.Comment> GetComments(int taskId);
    }
}