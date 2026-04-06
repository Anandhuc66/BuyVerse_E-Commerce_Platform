using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class UserAddressRepo : IUserAddressRepo
    {
        private readonly ApplicationDbContext _context;
        public UserAddressRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all user addresses
        public async Task<Result<List<UserAddress>>> GetAll()
        {
            var result = new Result<List<UserAddress>>();
            var list = await _context.UserAddressesSet
                .Include(a => a.User)
                .ToListAsync();

            if (list.Any())
                result.Response = list;
            else
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "No addresses found"
                });

            return result;
        }

        // Get addresses for a specific user
        public async Task<Result<List<UserAddress>>> GetByUserId(string userId)
        {
            var result = new Result<List<UserAddress>>();
            var list = await _context.UserAddressesSet
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .ToListAsync();

            if (list.Any())
                result.Response = list;
            else
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "No addresses found for this user"
                });

            return result;
        }

        // Get single address
        public async Task<Result<UserAddress>> GetById(int id)
        {
            var result = new Result<UserAddress>();
            var address = await _context.UserAddressesSet
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (address != null)
                result.Response = address;
            else
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "Address not found"
                });

            return result;
        }

        // Add new address
        public async Task<Result<UserAddress>> Add(UserAddressCreateDto model)
        {
            var result = new Result<UserAddress>();

            var address = new UserAddress
            {
                UserId = model.UserId,
                FullAddress = model.FullAddress,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Country = model.Country
            };

            _context.UserAddressesSet.Add(address);
            await _context.SaveChangesAsync();

            result.Response = address;
            result.Message = "Address added successfully";
            return result;
        }

        // Update address
        public async Task<Result<UserAddress>> Update(UserAddressUpdateDto model)
        {
            var result = new Result<UserAddress>();
            var address = await _context.UserAddressesSet.FindAsync(model.Id);

            if (address == null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "Address not found"
                });
                return result;
            }

            address.FullAddress = model.FullAddress;
            address.City = model.City;
            address.State = model.State;
            address.ZipCode = model.ZipCode;
            address.Country = model.Country;

            _context.UserAddressesSet.Update(address);
            await _context.SaveChangesAsync();

            result.Response = address;
            result.Message = "Address updated successfully";
            return result;
        }

        // Delete
        public async Task<Result<bool>> Delete(int id)
        {
            var result = new Result<bool>();
            var address = await _context.UserAddressesSet.FindAsync(id);

            if (address == null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "Address not found"
                });
                return result;
            }

            _context.UserAddressesSet.Remove(address);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "Address deleted successfully";
            return result;
        }
    }
}
