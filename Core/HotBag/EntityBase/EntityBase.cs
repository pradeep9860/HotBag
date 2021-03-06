﻿using AutoMapper;
using HotBag.AppUser;
using HotBag.Autofill.Attribute;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotBag.EntityBase
{
    public class EntityBase<TPrimaryKey> : IEntityBase<TPrimaryKey> 
    {
        #region Primary key of the table
        [Key]
        [BsonId]
        public TPrimaryKey Id { get; set; }
        #endregion

        //#region create audit entities
        //[IgnoreUpdate]
        //[BsonDateTimeOptions]
        //[IgnoreMap]
        //[AutoFill(AutoFillProperty.CurrentDate)]
        //public DateTime CreatedDateTime { get; set; } 
        //[IgnoreUpdate]
        //[IgnoreMap]
        //[ForeignKey("CreatedByUserEntity")]
        //public Guid? CreatedByUser { get; set; }
        //[IgnoreMap]
        //public virtual HotBagUser CreatedByUserEntity { get; set; }
        //#endregion

        //#region update audit entities
        //[IgnoreInsert]
        //[BsonDateTimeOptions]
        //[IgnoreMap]
        //[AutoFill(AutoFillProperty.CurrentDate)]
        //public DateTime? ModifiedDateTime { get; set; } 

        //[IgnoreInsert] 
        //[ForeignKey("UpdatedByUserEntity")]
        //[IgnoreMap]
        //public Guid? UpdatedByUser { get; set; }
        //[IgnoreMap]
        //public virtual HotBagUser UpdatedByUserEntity { get; set; }
        //#endregion
    }

    public class EntityBaseDto<TPrimaryKey> : IEntityBaseDto<TPrimaryKey>

    {
        #region Primary key of the table 
        public TPrimaryKey Id { get; set; }
        #endregion

        //#region create audit entities 
        //public DateTime CreatedDateTime { get; set; } 
        //public Guid? CreatedByUser { get; set; } 
        //#endregion

        //#region update audit entities 
        //public DateTime? ModifiedDateTime { get; set; } 
        //public Guid? UpdatedByUser { get; set; }  
        //#endregion
    }

}
