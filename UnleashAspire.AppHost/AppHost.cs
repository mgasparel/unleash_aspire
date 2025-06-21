var builder = DistributedApplication.CreateBuilder(args);

var unleashUser = builder.AddParameter("unleashUser", value: "unleash_user", secret: false);

// Add a postgres instance for unleash
var unleashPostgres = builder.AddPostgres("unleash-postgres", unleashUser);

// create the unleash database
var unleashDb = unleashPostgres.AddDatabase("unleash-postgres-db", "unleash");

// Add the unleash server
var unleash = builder.AddContainer("unleash", "unleashorg/unleash-server")
    .WithEnvironment(context =>
    {
        var unleashPostgresEndpoint = unleashPostgres.GetEndpoint("tcp");
        var unleashPostgresHost = unleashPostgresEndpoint.Property(EndpointProperty.Host);
        var unleashPostgresPort = unleashPostgresEndpoint.Property(EndpointProperty.Port);

        context.EnvironmentVariables["DATABASE_HOST"] = unleashPostgresHost;
        context.EnvironmentVariables["DATABASE_PORT"] = unleashPostgresPort;
        context.EnvironmentVariables["DATABASE_NAME"] = unleashDb.Resource.DatabaseName;
        context.EnvironmentVariables["DATABASE_USERNAME"] = unleashPostgres.Resource.UserNameParameter!.Value;
        context.EnvironmentVariables["DATABASE_PASSWORD"] = unleashPostgres.Resource.PasswordParameter!.Value;
        context.EnvironmentVariables["DATABASE_SSL"] = "false";
    })
    .WithEndpoint(
        "http",
        e =>
        {
            // default unleash port is 4242
            e.Port = 4242;
            e.TargetPort = 4242;
        })
    .WaitFor(unleashDb);


builder.Build().Run();
