using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASC.Utilities;
using ASC.Models.Models;
using ASC.Web.Areas.Configuration.Models;
using AutoMapper;
using ASC.Web.Controllers;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterDataOperations;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterDataOperations, IMapper mapper)
        {
            this._masterDataOperations = masterDataOperations;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterDataKeys = await _masterDataOperations.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterDataKeys);

            // Hold all Master Keys in session
            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);

            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel == null ? null : masterKeysViewModel.ToList(),
                IsEdit = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel masterKeysViewModel)
        {
            masterKeysViewModel.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys");
            if (!ModelState.IsValid)
            {
                return View(masterKeysViewModel);
            }
            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKeysViewModel.MasterDataKeyInContext);

            if (masterKeysViewModel.IsEdit)
            {
                // Update Master Key
                await _masterDataOperations.UpdateMasterKeyAsync(masterKeysViewModel.MasterDataKeyInContext.PartitionKey, masterKey);
            }
            else
            {
                // Insert Master Key
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = masterKey.Name;
                await _masterDataOperations.InsertMasterKeyAsync(masterKey);
            }

            return RedirectToAction("MasterKeys");
        }

        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            // Get All Master Keys and hold them in ViewBag for Select tag
            ViewBag.MasterKeys = await _masterDataOperations.GetAllMasterKeysAsync();
            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            // Get Master values based on master key.
            return Json(new { data = await _masterDataOperations.GetAllMasterValuesByKeyAsync(key) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid)
            {
                return Json("Error");
            }
            var masterDataValue = _mapper.Map<MasterDataValueViewModel,
            MasterDataValue>(masterValue);
            if (isEdit)
            {
                // Update Master Value
                await _masterDataOperations.UpdateMasterValueAsync(masterDataValue.PartitionKey,
                masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                // Insert Master Value
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                await _masterDataOperations.InsertMasterValueAsync(masterDataValue);
            }
            return Json(true);
        }
    }
}