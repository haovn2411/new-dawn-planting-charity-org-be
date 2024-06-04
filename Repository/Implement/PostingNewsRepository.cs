﻿using AutoMapper;
using BussinessObjects.Models;
using DAO;
using DTOS.News;
using DTOS.PostingDetail;
using Repository.Interface;

namespace Repository.Implement
{
    public class PostingNewsRepository : IPostingNewsRepository
    {
        private readonly IMapper mapper;

        public PostingNewsRepository(IMapper mapper)
        {
            this.mapper = mapper;
        }
        public async Task<bool> CreateNews(PostingNews postingNews)
        {
            var result = await PostingNewsDAO.Instance.CreateAsync(postingNews);
            return result;
        }

        public async Task<bool> DeleteNewsById(int id)
        {
            var newsEntity = await PostingNewsDAO.Instance.GetNewsById(id);
            if (newsEntity != null)
            {
                newsEntity.Status = 1;
                var result = await PostingNewsDAO.Instance.UpdateAsync(newsEntity);
                return result;
            }
            return false;
        }

        public async Task<IEnumerable<NewsType>> GetAllNewsByType(int typeID)
        {
            var news = new List<NewsType>();
            var postings = await PostingNewsDAO.Instance.GetAllNewsByType(typeID);
            foreach (var post in postings)
            {
                var images = await PostingDetailDAO.Instance.GetAllPostingNews(post.NewsID);
                var postingView = mapper.Map<NewsType>(post);
                postingView.Details = mapper.Map<IEnumerable<ResponsePostingDetail>>(images);
                news.Add(postingView);
            }
            return news;
        }

        public async Task<IEnumerable<NewsMonthView>> GetAllNewsEachMonth()
        {
            var news = PostingNewsDAO.Instance.GetAllAsync().Result
                .Where(y => y.Type == 1).ToList();

            return mapper.Map<IEnumerable<NewsMonthView>>(news);

        }

        public async Task<IEnumerable<PostingNews>> GetAllPostingNews ()
        {
            var responseNews = await PostingNewsDAO.Instance.GetAllAsync();
            return responseNews;
        }

        public async Task<PostingNews> GetNewsDetailById(int id)
        {
            var responseNews = await PostingNewsDAO.Instance.GetNewsById(id);
            return responseNews;
        }
    }
}
