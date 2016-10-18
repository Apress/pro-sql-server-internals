/****************************************************************************/
/*                       Pro SQL Server Internals                           */
/*      APress. 1st Edition. ISBN-13: 978-1430259626 ISBN-10:1430259620     */
/*                                                                          */
/*                  Written by Dmitri V. Korotkevitch                       */
/*                      http://aboutsqlserver.com                           */
/*                      dmitri@aboutsqlserver.com                           */
/****************************************************************************/
/*                        Chapters 13, 14. CLR                              */
/*                         CSV-List Aggregate.                              */
/*                 Code is based on example from MSDN                       */
/*         http://technet.microsoft.com/en-us/library/ms131056.aspx         */
/****************************************************************************/

using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text;
using System.IO;

[Serializable]
[SqlUserDefinedAggregate(
   Format.UserDefined,
   IsInvariantToNulls = true,
   IsInvariantToDuplicates = false,
   IsInvariantToOrder = false,
   MaxByteSize = -1
)]
public class Concatenate : IBinarySerialize
{
    // The buffer for the intermediate results
    private StringBuilder intermediateResult;

    // Initializes the buffer
    public void Init()
    {
        this.intermediateResult = new StringBuilder();
    }

    // Accumulate the next value if not null
    public void Accumulate(SqlString value)
    {
        if (value.IsNull)
            return;
        this.intermediateResult.Append(value.Value).Append(',');
    }

    // Merges the partiually completed aggregates
    public void Merge(Concatenate other)
    {
        this.intermediateResult.Append(other.intermediateResult);
    }

    // Called at the end of aggregation
    public SqlString Terminate()
    {
        string output = string.Empty;
        if (this.intermediateResult != null
            && this.intermediateResult.Length > 0)
        { // Deleting the trailing comma
            output = this.intermediateResult
               .ToString(0, this.intermediateResult.Length - 1);
        }
        return new SqlString(output);
    }

    // Deserializing data
    public void Read(BinaryReader r)
    {
        intermediateResult = new StringBuilder(r.ReadString());
    }

    // Serializing data
    public void Write(BinaryWriter w)
    {
        w.Write(this.intermediateResult.ToString());
    }
}
