using Business.Abstract;
using Business.BusinessAspect.Autofac;
using Business.CCS;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspect.Autofac.Validation;
using Core.Aspects.Autofac.Caching;
using Core.CrossCuttingConcerns.Validation;
using Core.Utilites.Business;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        IProductDal _productDal;
        ICategoryDal _categoryService;
        public ProductManager(IProductDal productDal, ICategoryDal categoryService)
        {
            _productDal = productDal;   
            _categoryService = categoryService;
        }
        [CacheAspect]
        public IDataResult<List<Product>> GetAll()
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(),Messages.ProductListed);    
        }

        public IDataResult<List<Product>> GetAllByCategoryId(int CategoryId)
        {
           return new SuccessDataResult<List<Product>>(_productDal.GetAll(p=>p.CategoryID==CategoryId),Messages.ProductListed);
        }

        public IDataResult<List<Product>> GetAllByUnitPrice(decimal min, decimal max)
        {   
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(p => p.UnitPrice >= min && p.UnitPrice <= max));
        }

        public IDataResult<List<ProductDetailDto>> GetProductDetails()
        {
            return new SuccessDataResult<List<ProductDetailDto>>(_productDal.GetProductDetails());
        }

        [SecuredOperation("product.add,admin")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public IResult Add(Product product)
            {
                IResult result = BusinessRules.Run( 
                    CheckIfProductCountOfCategoryCorrect(product.CategoryID),
                   CheckIfCategoryLimitExceded(),
                    CheckIfProductNameExists(product.ProductName));

                if (result != null)
                {
                    return result;
                }
                _productDal.Add(product);
                return new SuccessResult(Messages.ProductAdded);
        }

        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public IResult Update(Product product)
        {
            throw new NotImplementedException();
        }   
        [CacheAspect]
        public IDataResult<Product> GetById(int id)
        {
            return new SuccessDataResult<Product>(_productDal.Get(p=>p.ProductID==id));   
        }
        private IResult CheckIfProductCountOfCategoryCorrect(int CategoryId)
        {
            var result = _productDal.GetAll(p => p.CategoryID == CategoryId);
            if (result.Count >= 30)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }

            return new SuccessResult(Messages.ProductAdded);
        }
        private IResult CheckIfProductNameExists(string ProductName)
        {
            var result = _productDal.GetAll(p => p.ProductName == ProductName).Any();
            if (result)
            {
                return new ErrorResult(Messages.ProductNameAlreadyExists);
            }
            return new SuccessResult(Messages.ProductAdded);
        }
        private IResult CheckIfCategoryLimitExceded()
        {
            var result = _categoryService.GetAll();
            if (result.Count > 15)
            {
                return new ErrorResult(Messages.CategoryLimitExceded);
            }
            return new SuccessResult(Messages.ProductAdded);
        }
    }
}
