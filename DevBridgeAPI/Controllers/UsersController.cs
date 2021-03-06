﻿using Dapper;
using DevBridgeAPI.Models.Post;
using DevBridgeAPI.Models.Complex;
using DevBridgeAPI.Resources;
using DevBridgeAPI.Repository.Dao;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using DevBridgeAPI.UseCases.UserLogicN;
using System.Net.Http;
using System.Net;
using DevBridgeAPI.UseCases.Exceptions;
using DevBridgeAPI.Helpers;
using DevBridgeAPI.Models.Patch;
using DevBridgeAPI.Models.Misc;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Web.Helpers;

namespace DevBridgeAPI.Controllers
{
#pragma warning disable CA2000 // Dispose objects before losing scope

    public class UsersController : ApiController
    {
        private readonly IUserLogic userLogic;

        public UsersController(IUserLogic userLogic)
        {
            this.userLogic = userLogic;
        }

        [Authorize]
        [Route("api/users/current")]
        [HttpGet]
        public IHttpActionResult GetCurrentUser()
        {
            var identity = User.Identity;
            return Ok(userLogic.GetTeamTree(identity.Name).This);
        }

        /// <summary>
        /// Will register a new user with already assigned manager.
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 5: User with specified email has already completed registration<br/>
        /// 8: Request model is invalid
        /// </remarks>
        /// <param name="newUser">New user to be inserted into database</param>
        /// <returns>Described at responses</returns>
        [Authorize]
        [Route("api/users")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successful request, Return posted user", Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "Provided email already exists", Type = typeof(ErrorMessage))]
        [ValidateRequest]
        public IHttpActionResult RegisterUser([FromBody] User newUser)
        {
            return Ok(userLogic.RegisterNewUser(newUser));
        }

        /// <summary>
        /// Gets a tree represantation of team hierarchy
        /// starting from the specified root user
        /// </summary>
        /// <param name="rootUserId">Root user's ID that will be at the top of team hierarchy</param>
        /// <returns>A tree of users with subordinates as children starting from rootUser</returns>
        [Authorize]
        [Route("api/users/teamTree/{rootUserId}")]
        [HttpGet]
        [ResponseType(typeof(TeamTreeNode))]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successful request, Return team tree", Type = typeof(TeamTreeNode))]
        public IHttpActionResult GetTeamTree(int rootUserId)
        {
            return Ok(userLogic.GetTeamTree(rootUserId));
        }

        [Authorize]
        [Route("api/users/teamTree")]
        [HttpGet]
        [ResponseType(typeof(TeamTreeNode))]
        public IHttpActionResult GetTeamTree()
        {
            var identity = User.Identity;
            return Ok(userLogic.GetTeamTree(identity.Name));
        }

        /// <summary>
        /// Requests for a descendant user list 
        /// where users have at least 1 subordinate.
        /// Authenticated user will be the root user.
        /// </summary>
        /// <returns>Return a list of users</returns>
        [Authorize]
        [Route("api/users/managers")]
        [HttpGet]
        [ResponseType(typeof(TeamTreeNode))]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successful request, Return user list", Type = typeof(IEnumerable<User>))]
        public IHttpActionResult GetManagers()
        {
            var currUserId = User.Identity.GetId();
            return Ok(userLogic.GetDescendantTeamManagers(currUserId));
        }

        /// <summary>
        /// Changes restrictions for a specific user
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 6: User not found<br/>
        /// 8: Request model is invalid
        /// </remarks>
        /// <param name="userRestrictions">New restrictions, if ommited in request - will be set to null</param>
        /// <param name="userId">ID of user that is undergoing restriction changes</param>
        /// <returns>An updated user with changed restrictions</returns>
        [Route("api/users/restrictions/{userId}")]
        [HttpPatch]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successful request, Return user with changed restrictions", Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "User with provided ID not found", Type = typeof(ErrorMessage))]
        [ValidateRequest]
        public IHttpActionResult ChangeRestrictions([FromBody] UserRestrictions userRestrictions, int userId)
        {
            return Ok(userLogic.ChangeRestrictions(userRestrictions, userId));
        }

        /// <summary>
        /// Changes restrictions for every user
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 8: Request model is invalid
        /// </remarks>
        /// <param name="userRestrictions">New restrictions, if ommited in request - will be set to null</param>
        /// <returns>Nothing</returns>
        [Route("api/users/restrictions/global")]
        [HttpPatch]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Successful request")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        [SwaggerResponseRemoveDefaults]
        [ValidateRequest]
        public IHttpActionResult ChangeGlobalRestrictions([FromBody] UserRestrictions userRestrictions)
        {
            userLogic.ChangeGlobalRestrictions(userRestrictions);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Changes restrictions for every subordinate of a manager with ID = <paramref name="managerId"/>
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 6: Manager not found<br/>
        /// 8: Request model is invalid
        /// </remarks>
        /// <param name="userRestrictions">New restrictions, if ommited in request - will be set to null</param>
        /// <param name="managerId">ID of manager whose subordinates will have restrictions updated</param>
        /// <returns>Nothing</returns>
        [Route("api/users/restrictions/team/{managerId}")]
        [HttpPatch]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Successful request")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Manager with provided ID not found", Type = typeof(ErrorMessage))]
        [SwaggerResponseRemoveDefaults]
        [ValidateRequest]
        public IHttpActionResult ChangeTeamRestrictions([FromBody] UserRestrictions userRestrictions, int managerId)
        {
            userLogic.ChangeTeamRestrictions(userRestrictions, managerId);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        /// <summary>
        /// User with ID = <paramref name="userId"/> will be assigned a new manager with ID = <paramref name="newManagerId"/>
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 1: Attempted self manager assignment<br/>
        /// 2: User not found<br/>
        /// 3: Manager not found<br/>
        /// 4: Manager reassignment would cause cycles in relationships
        /// </remarks>
        /// <param name="newManagerId">ID of a new manager</param>
        /// <param name="userId">ID of a user that will be assigned a specified manager</param>
        /// <returns>Updated User model with newly assigned manager</returns>
        [Route("api/users/manager/{userId}")]
        [HttpPatch]
        [ValidateRequest]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successful request, Return user with changed manager", Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        public IHttpActionResult ChangeTeamManager([FromBody] UserManagerId newManagerId, int userId)
        {
            return Ok(userLogic.ChangeTeamMember(newManagerId.ManagerId.Value, userId));  
        }

        /// <summary>
        /// Will update unregistered user's credentials. Used for finishing registration
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 6: User with provided token not found<br/>
        /// 9: Invalid RegistrationToken (could be replaced by consecutive invitations)<br/>
        /// 10: RegistrationToken is expired<br/>
        /// 11: User is already registered
        /// </remarks>
        /// <param name="regCredentials">Credentials supplied for validation and setting user's password</param>
        /// <returns>Updated User model after registering</returns>
        [Route("api/users/finishReg")]
        [HttpPatch]
        [ValidateRequest]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successful request, Return registered user", Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "User with provided registration token not found", Type = typeof(ErrorMessage))]
        public IHttpActionResult FinishRegistration([FromBody] RegCredentials regCredentials)
        {
            return Ok(userLogic.FinishRegistration(regCredentials));
        }

        /// <summary>
        /// Requests user by provided registration token
        /// (The token that is sent by invitation email)
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 6: User with provided token not found<br/>
        /// 10: RegistrationToken is expired<br/>
        /// </remarks>
        /// <param name="regToken">Registration token (mandatory parameter) for user lookup</param>
        /// <returns>Requsted user data</returns>
        [Route("api/users/regToken")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successful request, Return user", Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "User with provided registration token not found", Type = typeof(ErrorMessage))]
        public IHttpActionResult GetByRegistrationToken([FromBody] RegistrationTokenWrapper regToken)
        {
            return Ok(userLogic.GetByRegistrationToken(regToken.RegistrationToken));
        }

        /// <summary>
        /// Request for user password to be changed
        /// Requires old password for identity validation
        /// </summary>
        /// <remarks>
        /// Error codes:<br/>
        /// 6: User with provided ID not found<br/>
        /// 14: Invalid OldPassword<br/>
        /// </remarks>
        /// <param name="changePassword">Old and new passwords</param>
        /// <response code="204">Password changed successfully</response>
        [Route("api/users/changePassword")]
        [HttpPatch]
        [Authorize]
        [ValidateRequest]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Password changed successfully")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Request failed validations", Type = typeof(ErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "User was not found", Type = typeof(ErrorMessage))]
        public IHttpActionResult ChangePassword([FromBody] ChangePassword changePassword)
        {
            var userId = User.Identity.GetId();
            userLogic.ChangePassword(userId, changePassword.OldPassword, changePassword.NewPassword);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
#pragma warning restore CA2000 // Dispose objects before losing scope
}
