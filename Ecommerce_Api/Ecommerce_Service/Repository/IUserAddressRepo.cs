using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public interface IUserAddressRepo
    {
        Task<Result<List<UserAddress>>> GetAll();
        Task<Result<List<UserAddress>>> GetByUserId(string userId);
        Task<Result<UserAddress>> GetById(int id);

        Task<Result<UserAddress>> Add(UserAddressCreateDto model);
        Task<Result<UserAddress>> Update(UserAddressUpdateDto model);
        Task<Result<bool>> Delete(int id);
    }
}
