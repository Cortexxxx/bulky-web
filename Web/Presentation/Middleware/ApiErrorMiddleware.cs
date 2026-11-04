using System.Net;
using Application.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace Web.Presentation.Middleware;

public static class ApiErrorMiddleware
{
    public static void UseApiErrors(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            try
            {
                await (next?.Invoke(context) ?? Task.CompletedTask);
            }
            catch (EmailAlreadyExistsException)
            {
                context.Response.StatusCode = HttpStatusCode.Conflict;
                context.Response.WriteJson(new { email = (string[]) ["email_exists"] });
            }
            catch (NoActiveSessionsException)
            {
                context.Response.StatusCode = HttpStatusCode.Unauthorized;
                context.Response.WriteJson(new { error = (string[]) ["unauthorized"] });
            }
            catch (WrongEmailOrPasswordException)
            {
                context.Response.StatusCode = HttpStatusCode.Unauthorized;
                context.Response.WriteJson(new { error = (string[]) ["wrong_email_or_password"] });
            }
            catch (MuscleGroupExistsException)
            {
                context.Response.StatusCode = HttpStatusCode.Conflict;
                context.Response.WriteJson(new { muscle_group = (string[]) ["muscle_group_exists"] });
            }
            catch (ExerciseExistsException)
            {
                context.Response.StatusCode = HttpStatusCode.Conflict;
                context.Response.WriteJson(new { exercise = (string[]) ["exercise_exists"] });
            }
            catch (UnknownMuscleGroupException)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                context.Response.WriteJson(new { error = (string[]) ["unknown_muscle_group"] });
            }
            catch (ValidationException e)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                var obj = e.Errors
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(f => f.Key.ToLower(), 
                        failures => failures
                            .Select(f => f.ErrorCode)
                            .ToArray());
                context.Response.WriteJson(obj);
            }
        });
    }
}