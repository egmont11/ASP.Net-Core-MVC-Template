# ASP.Net Core MVC Template
This project is a template for creating a new ASP.Net Core MVC project with PostgreSQL database and MSTest project.

It has an Auth controller for handling user authentication and authorization. 
There are two roles — Admin and User. Can be expanded in the UserModel file, which has the UserRole enum.

There is an admin area with a simple dashboard and user overview. Can be expanded for editing users, but 
right now does not have any functionality besides viewing them.

Secrets are managed via the .env file. An example of loading configuration from this file is in Program.cs, where it
loads the connection string for the database.

Users can view their own data, edit it, download their data in .json format as well as delete their account.

## Project structure.

Entities folder — contains the models for the database.

Models folder — a class for passing errors.

Data folder — houses the appDbContext and seeder for default data. There you can set the default admin credentials.

The rest should be self-explanatory.

## Running the project in a development environment.

The project is configured so that it is easy to run in a development environment as well as in a production environment.
In the development environment, the project is configured to use a local PostgreSQL database via docker compose.
For production, you change the connection string in the .env file.

1. Have Docker installed.
2. Run the docker compose file. (docker-compose up --build)
3. Run and edit the project using your IDE or editor of choice.