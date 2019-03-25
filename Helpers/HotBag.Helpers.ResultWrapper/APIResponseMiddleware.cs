﻿using HotBag.ResultWrapper.Extensions;
using HotBag.ResultWrapper.Filters;
using HotBag.ResultWrapper.WrapperModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace HotBag.ResultWrapper
{
    public class APIResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public APIResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        { 
            if (IsSwagger(context))
                await this._next(context);
            else
            {
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    try
                    { 
                        await _next.Invoke(context);

                        var ModelState = context.Features.Get<ModelStateFeature>()?.ModelState;
                     
                        if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            var body = await FormatResponse(context.Response);
                            await HandleSuccessRequestAsync(context, body, context.Response.StatusCode);

                        }
                        else if (ModelState != null && !ModelState.IsValid)
                        {
                            await HandleModelStateRequestAsync(context, context.Response.StatusCode, ModelState);
                        }
                        else
                        {
                            await HandleNotSuccessRequestAsync(context, context.Response.StatusCode);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        await HandleExceptionAsync(context, ex);
                    }
                    finally
                    {
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
            }

        }

        private static Task HandleExceptionAsync(HttpContext context, System.Exception exception)
        {
            ApiError apiError = null;
            APIResponse apiResponse = null;
            int code = 0;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                apiError = new ApiError(ex.Message);
                apiError.ValidationErrors = ex.Errors;
                apiError.ReferenceErrorCode = ex.ReferenceErrorCode;
                apiError.ReferenceDocumentLink = ex.ReferenceDocumentLink;
                code = ex.StatusCode;
                context.Response.StatusCode = code;

            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError("Unauthorized Access");
                code = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusCode = code;
            }
            else if(exception is Exception)
            {
                apiError = new ApiError(exception.Message);
                code = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = code;
            } 
            else
            {
#if !DEBUG
                var msg = "An unhandled error occurred.";
                string stack = null;
#else
                var msg = exception.GetBaseException().Message;
                string stack = exception.StackTrace;
#endif

                apiError = new ApiError(msg);
                apiError.Details = stack;
                code = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = code;
            }

            context.Response.ContentType = "application/json";

            apiResponse = new APIResponse(code, ResponseMessageEnum.Exception.GetDescription(), null, false, apiError);

            var json = JsonConvert.SerializeObject(apiResponse);

            return context.Response.WriteAsync(json);
        }

        private static Task HandleNotSuccessRequestAsync(HttpContext context, int code)
        {
            context.Response.ContentType = "application/json";

            ApiError apiError = null;
            APIResponse apiResponse = null;

            if (code == (int)HttpStatusCode.NotFound)
                apiError = new ApiError("The specified URI does not exist. Please verify and try again.");
            else if (code == (int)HttpStatusCode.NoContent)
                apiError = new ApiError("The specified URI does not contain any content.");
            else if(code == (int)HttpStatusCode.Unauthorized)
                apiError = new ApiError("User not loged in.");
            else if(code == (int)HttpStatusCode.BadRequest)
            {
                var ModelState = context.Features.Get<ModelStateFeature>()?.ModelState;
                apiError = new ApiError(ModelState);
            } 
            else
                apiError = new ApiError("Your request cannot be processed. Please contact a support.");

            apiResponse = new APIResponse(code, ResponseMessageEnum.Failure.GetDescription(), null, false, apiError);
            context.Response.StatusCode = code;

            var json = JsonConvert.SerializeObject(apiResponse);

            return context.Response.WriteAsync(json);
        }

        private static Task HandleModelStateRequestAsync(HttpContext context, int code, ModelStateDictionary modelState)
        {
            context.Response.ContentType = "application/json";

            ApiError apiError = null;
            APIResponse apiResponse = null;

            if (code == (int)HttpStatusCode.NotFound)
                apiError = new ApiError("The specified URI does not exist. Please verify and try again.");
            else if (code == (int)HttpStatusCode.NoContent)
                apiError = new ApiError("The specified URI does not contain any content.");
            else if (code == (int)HttpStatusCode.BadRequest)
                apiError = new ApiError(modelState);
            else
                apiError = new ApiError("Your request cannot be processed. Please contact a support.");

            apiResponse = new APIResponse(code, ResponseMessageEnum.Failure.GetDescription(), null, false, apiError);
            context.Response.StatusCode = code;

            var json = JsonConvert.SerializeObject(apiResponse);

            return context.Response.WriteAsync(json);
        }

        private static Task HandleSuccessRequestAsync(HttpContext context, object body, int code)
        {
            context.Response.ContentType = "application/json";
            string jsonString, bodyText = string.Empty;
            APIResponse apiResponse = null;


            if (!body.ToString().IsValidJson())
                bodyText = JsonConvert.SerializeObject(body);
            else
                bodyText = body.ToString();

            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);
            Type type;

            type = bodyContent?.GetType();

            if (type.Equals(typeof(Newtonsoft.Json.Linq.JObject)))
            {
                apiResponse = JsonConvert.DeserializeObject<APIResponse>(bodyText);
                if (!apiResponse.IsSuccess)
                {
                    apiResponse = new APIResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, true, null);
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                }
                if (apiResponse.StatusCode != code)
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                else if (apiResponse.Result != null)
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                else
                {
                    apiResponse = new APIResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, true, null);
                    jsonString = JsonConvert.SerializeObject(apiResponse);
                }
            }
            else
            {
                apiResponse = new APIResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, true, null);
                jsonString = JsonConvert.SerializeObject(apiResponse);
            }

            return context.Response.WriteAsync(jsonString);
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return plainBodyText;
        }

        private bool IsSwagger(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/swagger");

        }

    }
}
