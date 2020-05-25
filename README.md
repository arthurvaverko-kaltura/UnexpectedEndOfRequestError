# Reproduction of reading buffered request body in netcore 3.1

spin up the server
`dotnet run`

create post body payload
`echo some body once told me > body.txt`

run appache bench
`ab -p body.txt -c 1000 -n 1000 http://localhost:5000/`





