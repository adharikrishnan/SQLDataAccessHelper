SQL Data Access Helper Core
===========
  [![License LGPLv3](https://img.shields.io/badge/license-Apache%20License%202.0-blue)](http://www.apache.org/licenses/LICENSE-2.0)

## Overview

A Helper Library for  SQL Server and PostgreSQL to help streamline database interactions, suitable for database-first architectures.

- SQL Connection, Commands and Data Reader Lifetimes are fully managed by the library classes.
- Consumers only need to worry about passing in the correct SQL Commands, Command Type and Parameters.
- Supports standard Reader, Scalar and Non Query Operations for both SQL Server and PostgreSQL, with both Synchronous and Asynchronous methods for the three.
- Supports opening both standard connections with both read and write operations, and readonly connections with only readonly connections 

## Example

```csharp
using SqlDataAccessHelper.Core.SQLServer;

SqlServerDataAccess dataAccess = new SqlServerDataAccess("Your Connection String");

using (dataAccess.Open())
{
    var dataReader = dataAccess.ExecuteReader(CommandType.Text, "Your Query", yourParameters);
    
    if(dataReader.HasRows)
    {
        while(dataReader.Read())
        {
            // Read thae data as required
        }
    }
    // If another reader is open for the same connection, the class instance will automatically close 
    // and dispose of the previous.
    var dataReader2 = dataAccess.ExecuteReader(CommandType.StoredProcedure, "Your Query", yourParameters);
}
// Any open readers and commands are disposed along with the connection.
```
## License
Copyright © 2025 Advaith H.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
