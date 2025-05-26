using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupsController : ControllerBase
    {

        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet("GetAccountGroups")]
        public async Task<ActionResult<List<GroupDto>>> GetUserGroups(string token)
        {
            try
            {
                var result = await _groupService.GetGroups(token); // await is essentieel!
                return Ok(result); // dit moet een gewone List<GroupDto> zijn


            }
            catch (Exception ex)
            {
                return BadRequest("GetUserGroups triggered een exception: "+ex.Message);
            }
        }

        [HttpGet("GetWhitelistedGroups")]
        public async Task<ActionResult<List<GroupDto>>> GetWhitelistedGroups(int traderId)
        {
            try
            {
                var result = await _groupService.GetWhitelistedGroups(traderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("WhitelistGroups")]
        public async Task<IActionResult> WhitelistGroupId(int traderId, string groupId, string groupName)
        {
            try
            {
                await _groupService.WhitelistGroup(traderId, groupId, groupName);
                return Ok();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeWhitelistGroups")]
        public async Task<IActionResult> DeWhitelistGroupId(string traderId, string groupId)
        {
            try
            {
                await _groupService.DeleteWhitelistedGroup(traderId, groupId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
