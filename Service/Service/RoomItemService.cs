using BusinessObject.DTO.RoomItem;
using BusinessObject.Models;
using Repo.IRepository;
using Service.Exceptions;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class RoomItemService : IRoomItemService
    {
        private readonly IRoomItemRepository _repo;
        public RoomItemService(IRoomItemRepository repo) { _repo = repo; }

        public Task<RoomItem> GetAsync(Guid id) { return _repo.GetByIdAsync(id); }

        public Task<List<RoomItemListItemDto>> ListCatalogAsync()
        {
            // Trả defaultX/Y
            return _repo.ListAllAsync().ContinueWith(t =>
            {
                var list = new List<RoomItemListItemDto>();
                if (t.Result != null)
                {
                    foreach (var x in t.Result)
                    {
                        list.Add(new RoomItemListItemDto
                        {
                            Id = x.RoomItemId,
                            RoomType = x.RoomType,
                            Name = x.Item,
                            X = x.DefaultX,
                            Y = x.DefaultY
                        });
                    }
                }
                return list;
            });
        }

        public Task<List<RoomItemListItemDto>> ListInRoomAsync(Guid roomId)
        {
            return _repo.ListInRoomAsync(roomId);
        }

        public async Task<List<RoomItem>> CreateAsync(IEnumerable<AddRoomItemRequest> reqs)
        {
            var list = reqs.Select(r => new RoomItem
            {
                Item = r.Name,
                SubName = r.SubName,
                RoomType = r.RoomType,
                DefaultX = r.X,
                DefaultY = r.Y,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            // gọi đúng hàm add list
            return await _repo.AddAsync(list);
        }

        public async Task<RoomItem> UpdateAsync(Guid id, UpdateRoomItemRequest dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            var changed = false;

            if (dto.Name != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    entity.Item = dto.Name.Trim();

                changed = true;
            }

            if (dto.SubName != null)
            {
                entity.SubName = dto.SubName; 
                changed = true;
            }

            if (dto.RoomType != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.RoomType))
                    entity.RoomType = dto.RoomType.Trim();

                changed = true;
            }

            if (dto.X.HasValue)
            {
                entity.DefaultX = dto.X;
                changed = true;
            }

            if (dto.Y.HasValue)
            {
                entity.DefaultY = dto.Y;
                changed = true;
            }

            if (!changed)
            {
                return entity;
            }

            entity.UpdatedAt = DateTime.UtcNow;
            return await _repo.UpdateAsync(entity);
        }



        public Task<bool> DeleteAsync(Guid id) { return _repo.DeleteAsync(id); }
    }
}
