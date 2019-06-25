using System;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.Entities;
using Zpp.WrappersForPrimitives;

namespace Zpp.Wrappers
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface IDemandLogic
    {
       
        Provider CreateProvider(IDbCache dbCache);

        // is needed to compare two instances
        DueTime GetDueTime();

        IDemand ToIDemand();
    }
}