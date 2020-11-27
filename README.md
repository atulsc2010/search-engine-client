# Search Engine Challenge: C#

#### How to Run SearchEngine queries
```
docker-compose down --remove-orphans
docker-compose up --build

1. First Time Query - Cached = false Occurrances > 0 
http://localhost:8000/api/search/google/e-settlements/sympli.com.au
http://localhost:8000/api/search/google/e-settlements/pexa.com.au
http://localhost:8000/api/search/bing/e-settlements/sympli.com.au
http://localhost:8000/api/search/bing/e-settlements/pexa.com.au

2. Second Time Query -  Cached = true Occurrances > 0
http://localhost:8000/api/search/google/e-settlements/sympli.com.au
http://localhost:8000/api/search/google/e-settlements/pexa.com.au


3. Second Time Query After One Hour - Cached = false Occurrances > 0
http://localhost:8000/api/search/google/e-settlements/sympli.com.au
http://localhost:8000/api/search/google/e-settlements/pexa.com.au


4. Keyword Not found - Occurrances = 0 
http://localhost:8000/api/search/google/music/sympli.com.au

5. Find Url Not found - Occurrances = 0 
http://localhost:8000/api/search/google/e-settlements/jenga.com.au

6. Unknown Engine -  Occurrances > 0  using default search engine
http://localhost:8000/api/search/netscape/e-settlements/sympli.com.au
```

### Overview

This application follows standard ASP.Net WebAPI structure with CQRS design pattern.

Project uses MediatR package for separation of Domain and Application logic.   
All the application logic and business rules are implemented in the respective Handlers.
The Controllers are only responsible for accepting a request and delegating them to MediatR to route them to the correct handler.
  

Application Domain contains 2 main entities User and Account.
This project uses PostgreSQL for data persistance.


    Folder Structure

    TestProject.WebAPI
        /Domain -  Domain Entity classes
        /Commands - Command/Request/Reponse type definitions per entity 
        /Queries - Query/Request/Reponse type definitions per entity
        /Handlers - This is the application layer which implement the required business logic and db operations  
        /Integration - HTTP client classes
        /Controllers
            /SearchController - API for search engine
        /Models - Contains Database context 
    

## Building

Build using IDE or CLI

    Only application build then use
        dotnet build
    
    Docker Build as below 
        cd /source/repos/users-api/TestProject.WebAPI
        docker-compose up --build 

    DB Migrations are added into Application startup


## Deploying

This project has Docker support, containers can be deployed using `docker-compose` shown in above sections.

## Additional Information
### Further Improvements 
- Use a standard API media type such as JsonApi for standardised serialization instead of custom JSON for every API
- Application Logging for better diagnosis
- Use AutoMapper for concise code for mapping between domain and request/response objects
- Create a repository for handling DB oprations so that it can be separted from handlers.
- Add Swagger support for better API documnetation
- Move DB Migrations out of application start up for better performance




## Testing of Older APIs

This api can be tested in following ways 
    
1. By Running Unit Tests
    ```
        Via CLI 

            cd /source/repos/users-api/TestProject.Tests
            dotnet test

        Or Use a test runner in the IDE 
    ```
2. By Running Api as Docker container and use PostMan or Curl for sending requests 
    ```
        In CLI , which will build and run the application and db containers

        cd /source/repos/users-api/TestProject.WebAPI
        docker-compose up --build 

    ```  

 Once the app and db containers are running we can make following API requests 

##### Health check 
    
Request
```
    GET http://localhost:8000/api/users
```
    
Response
```
        200 OK
        Api is running.
```       

##### Create User 
    
Request
```
    curl --location --request POST 'http://localhost:8000/api/users/create' \
    --header 'Content-Type: application/json' \
    --data-raw '{"name": "test" , "email": "test@email.com", "salary": 10000 , "expenses": 1500}'
```
    
Response
```
    201 Created

    {
        "type": "CreateUserResponse",
        "id": "48d9159d-2b2d-4531-b99b-670afc1971c7",
        "status": "Success"
    }
```       

##### Create Account 
    
Request
```
    curl --location --request POST 'http://localhost:8000/api/accounts/create' \
    --header 'Content-Type: application/json' \
    --data-raw '{
        "userid": "48d9159d-2b2d-4531-b99b-670afc1971c7"
    }'
```
    
Response
```
    201 Created

    {
        "type": "CreateAccountResponse",
        "id": "ec899376-3718-4b3b-aa75-8012b9fdcb5d",
        "status": "Success"
    }
```    

##### List Users 
    
Request
```
    curl --location --request GET 'http://localhost:8000/api/users/list'
```
    
Response
```
    200 Ok

    [
        {
            "type": "ListUsersResponse",
            "id": "47f144d4-1c89-4e42-95a7-9014b82f2570",
            "name": "test",
            "email": "test1@email.com"
        },
        {
            "type": "ListUsersResponse",
            "id": "7eebd0f6-a2af-4a82-958b-a28bd52b05c8",
            "name": "test",
            "email": "test10@email.com"
        }
    ]


``` 

##### Get User 
    
Request
```
    curl --location --request GET 'http://localhost:8000/api/users/7eebd0f6-a2af-4a82-958b-a28bd52b05c8'
```
    
Response
```
    200 Ok

    {
        "type": "GetUserResponse",
        "id": "7eebd0f6-a2af-4a82-958b-a28bd52b05c8",
        "name": "test",
        "email": "test10@email.com",
        "salary": 10000,
        "expenses": 1500,
        "accounts": [
            {
                "id": "1a2b9b1d-b5b5-4753-a1d6-6f7c8c0609c5",
                "name": "Wallet 9981509",
                "status": 0,
                "balance": 0,
                "userId": "7eebd0f6-a2af-4a82-958b-a28bd52b05c8"
            },
            {
                "id": "ec899376-3718-4b3b-aa75-8012b9fdcb5d",
                "name": "Wallet 0035283",
                "status": 0,
                "balance": 0,
                "userId": "7eebd0f6-a2af-4a82-958b-a28bd52b05c8"
            }
        ]
    }

```    

##### List Accounts 
    
Request
```
    curl --location --request GET 'http://localhost:8000/api/accounts/list'
```

```
    200 OK

    [
        {
            "type": "ListAccountsResponse",
            "id": "9b4f9d12-731e-4361-a5aa-b976e4c7ac8e",
            "userId": "7a8419ba-ab3c-4a3c-8241-84acafecb667",
            "name": "Wallet 2808765",
            "status": 0,
            "balance": 0
        },
        {
            "type": "ListAccountsResponse",
            "id": "32739e8e-6f1f-4eec-8460-37db99df810a",
            "userId": "7a8419ba-ab3c-4a3c-8241-84acafecb667",
            "name": "Wallet 9563500",
            "status": 0,
            "balance": 0
        },
    ]
```


