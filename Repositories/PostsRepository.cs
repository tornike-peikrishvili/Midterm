using Microsoft.EntityFrameworkCore;
using Reddit.Models;
using System.Linq.Expressions;

namespace Reddit.Repositories
{
    public class PostsRepository : IPostsRepository
    {
        private readonly ApplicationDbContext _context;

        public PostsRepository(ApplicationDbContext applcationDBContext)
        {
            _context = applcationDBContext;
        }
        public async Task<PagedList<Post>> GetPosts(int pageNumber, int pageSize, string? searchTerm, string? sortTerm = null, bool isAscending = true)
        {
            var posts = _context.Posts.AsQueryable();

            if (searchTerm != null)
            {
                posts = posts.Where(p => p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm));
            }

            // Sort
            if (isAscending)
            {
                posts = posts.OrderBy(GetSortExpression(sortTerm));
            }
            else
            {
                posts = posts.OrderByDescending(GetSortExpression(sortTerm));
            }

            // Pagination 
            var paginatedPosts = await posts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return await PagedList<Post>.CreateAsync(paginatedPosts, posts.Count(), pageNumber, pageSize);
        }

        private Expression<Func<Post, object>> GetSortExpression(string? sortTerm)
        {
            sortTerm = sortTerm?.ToLower();
            return sortTerm switch
            {
                "positivity" => post => (post.Upvote) / (post.Upvote + post.Downvote),
                "popular" => post => post.Upvote + post.Downvote,
                _ => post => post.Id
            };
        }
    }
}
