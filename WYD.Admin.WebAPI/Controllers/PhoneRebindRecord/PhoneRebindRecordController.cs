using Microsoft.AspNetCore.Mvc;
using WYD.Infrastructure.Attributes;
using WYD.Infrastructure.Controllers;
using WYD.Infrastructure.WebExtensions;
using WYD.Model.Dto.PhoneRebindRecord;
using WYD.Service.IService.PhoneRebindRecord;

namespace WYD.Admin.WebAPI.Controllers.PhoneRebindRecord
{
    /// <summary>
    /// 电话号码换绑记录
    /// </summary>
    [Route("system/phone-rebind")]
    [ApiController]
    public class PhoneRebindRecordController : ApiBaseController
    {
        private readonly IPhoneRebindRecordService _service;

        public PhoneRebindRecordController(IPhoneRebindRecordService service)
        {
            _service = service;
        }

        /// <summary>
        /// 分页查询换绑记录
        /// </summary>
        [HttpGet("page")]
        [ActionPermissionFilter(Permission = "system:phoneRebind:list")]
        [Log(Title = "查询换绑记录")]
        public async Task<IActionResult> GetPageList([FromQuery] PhoneRebindRecordQueryDto query)
        {
            var result = await _service.GetPageListAsync(query);
            return OK(result);
        }

        /// <summary>
        /// 根据ID获取详情
        /// </summary>
        [HttpGet("{id:guid}")]
        [ActionPermissionFilter(Permission = "system:phoneRebind:list")]
        [Log(Title = "查看换绑记录")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? ERROR("记录不存在") : OK(result);
        }

        /// <summary>
        /// 新增换绑记录
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "system:phoneRebind:add")]
        [Log(Title = "新增换绑记录")]
        public async Task<IActionResult> Create([FromBody] PhoneRebindRecordCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OldPhone) || string.IsNullOrWhiteSpace(dto.NewPhone))
                return ERROR("旧号码与新号码不能为空");
            if (string.IsNullOrWhiteSpace(dto.AppName))
                return ERROR("绑定应用名称不能为空");

            var createdBy = HttpContext.GetName();
            var id = await _service.CreateAsync(dto, createdBy);
            return OK(new { Id = id }, "新增成功");
        }

        /// <summary>
        /// 更新换绑记录
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "system:phoneRebind:edit")]
        [Log(Title = "更新换绑记录")]
        public async Task<IActionResult> Update([FromBody] PhoneRebindRecordUpdateDto dto)
        {
            if (dto.Id == Guid.Empty) return ERROR("主键ID不能为空");
            if (string.IsNullOrWhiteSpace(dto.OldPhone) || string.IsNullOrWhiteSpace(dto.NewPhone))
                return ERROR("旧号码与新号码不能为空");

            var updatedBy = HttpContext.GetName();
            var ok = await _service.UpdateAsync(dto, updatedBy);
            return ok ? OK(null, "更新成功") : ERROR("记录不存在或已被删除");
        }

        /// <summary>
        /// 删除换绑记录（软删除）
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ActionPermissionFilter(Permission = "system:phoneRebind:remove")]
        [Log(Title = "删除换绑记录")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deletedBy = HttpContext.GetName();
            var ok = await _service.DeleteAsync(id, deletedBy);
            return ok ? OK(null, "删除成功") : ERROR("记录不存在或已被删除");
        }

        /// <summary>
        /// 批量删除（软删除）
        /// </summary>
        [HttpDelete("batch")]
        [ActionPermissionFilter(Permission = "system:phoneRebind:remove")]
        [Log(Title = "批量删除换绑记录")]
        public async Task<IActionResult> BatchDelete([FromBody] Guid[] ids)
        {
            if (ids == null || ids.Length == 0) return ERROR("请选择要删除的记录");

            var deletedBy = HttpContext.GetName();
            var ok = await _service.BatchDeleteAsync(ids, deletedBy);
            return ok ? OK(null, "批量删除成功") : ERROR("所选记录不存在或已被删除");
        }
    }
}
