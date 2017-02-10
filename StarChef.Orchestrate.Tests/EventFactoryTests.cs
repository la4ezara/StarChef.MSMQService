﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace StarChef.Orchestrate.Tests
{
    using SourceSystem = Fourth.Orchestration.Model.Menus.Events.SourceSystem;
    using ChangeType = Fourth.Orchestration.Model.Menus.Events.ChangeType;

    using IngredientUpdated = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated;
    using RecipeUpdated = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated;
    using GroupUpdated = Fourth.Orchestration.Model.Menus.Events.GroupUpdated;
    using MenuUpdated = Fourth.Orchestration.Model.Menus.Events.MenuUpdated;
    using MealPeriodUpdated = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated;
    using SupplierUpdated = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated;
    using UserUpdated = Fourth.Orchestration.Model.Menus.Events.UserUpdated;

    using IngredientUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated.Builder;
    using RecipeUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated.Builder;
    using GroupUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.GroupUpdated.Builder;
    using MenuUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MenuUpdated.Builder;
    using MealPeriodUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated.Builder;
    using SupplierUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated.Builder;
    using UserUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.UserUpdated.Builder;

    public class EventFactoryTests
    {
        #region Delete events

        [Fact]
        public void Should_create_delete_event_for_IngredientUpdated()
        {
            var eventSetter = new Mock<IEventSetter<IngredientUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForDelete(It.IsAny<IngredientUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<IngredientUpdatedBuilder, string, int>(SetMandatoryFields_forDelete);

            var eventFactory = new EventFactory(
                eventSetter.Object,
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateDeleteEvent<IngredientUpdated, IngredientUpdatedBuilder>("any", "any", 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.DELETE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_delete_event_for_RecipeUpdated()
        {
            var eventSetter = new Mock<IEventSetter<RecipeUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForDelete(It.IsAny<RecipeUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<RecipeUpdatedBuilder, string, int>(SetMandatoryFields_forDelete);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateDeleteEvent<RecipeUpdated, RecipeUpdatedBuilder>("any", "any", 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.DELETE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_delete_event_for_GroupUpdated()
        {
            var eventSetter = new Mock<IEventSetter<GroupUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForDelete(It.IsAny<GroupUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<GroupUpdatedBuilder, string, int>(SetMandatoryFields_forDelete);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateDeleteEvent<GroupUpdated, GroupUpdatedBuilder>("any", "any", 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.DELETE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_delete_event_for_MenuUpdated()
        {
            var eventSetter = new Mock<IEventSetter<MenuUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForDelete(It.IsAny<MenuUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<MenuUpdatedBuilder, string, int>(SetMandatoryFields_forDelete);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateDeleteEvent<MenuUpdated, MenuUpdatedBuilder>("any", "any", 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.DELETE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_delete_event_for_MealPeriodUpdated()
        {
            var eventSetter = new Mock<IEventSetter<MealPeriodUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForDelete(It.IsAny<MealPeriodUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<MealPeriodUpdatedBuilder, string, int>((builder, s, i) =>
                {
                    SetMandatoryFields_forDelete(builder, s, i);
                    SetMandatoryFields_forDelete_forMealPeriod(builder, s, i);
                });

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateDeleteEvent<MealPeriodUpdated, MealPeriodUpdatedBuilder>("any", "any", 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.DELETE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_delete_event_for_SupplierUpdated()
        {
            var eventSetter = new Mock<IEventSetter<SupplierUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForDelete(It.IsAny<SupplierUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<SupplierUpdatedBuilder, string, int>(SetMandatoryFields_forDelete);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateDeleteEvent<SupplierUpdated, SupplierUpdatedBuilder>("any", "any", 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.DELETE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_delete_event_for_UserUpdated()
        {
            var eventSetter = new Mock<IEventSetter<UserUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForDelete(It.IsAny<UserUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<UserUpdatedBuilder, string, int>(SetMandatoryFields_forDelete);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                eventSetter.Object);

            var eventObject = eventFactory.CreateDeleteEvent<UserUpdated, UserUpdatedBuilder>("any", "any", 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.DELETE, eventObject.ChangeType);
        }

        #endregion

        #region Update events

        [Fact]
        public void Should_create_update_event_for_IngredientUpdated()
        {
            var eventSetter = new Mock<IEventSetter<IngredientUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<IngredientUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<IngredientUpdatedBuilder, string, int, int>(SetMandatoryFields_forUpdate);

            var eventFactory = new EventFactory(
                eventSetter.Object,
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateUpdateEvent<IngredientUpdated, IngredientUpdatedBuilder>("any", 0, 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.UPDATE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_update_event_for_RecipeUpdated()
        {
            var eventSetter = new Mock<IEventSetter<RecipeUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<RecipeUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<RecipeUpdatedBuilder, string, int, int>(SetMandatoryFields_forUpdate);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateUpdateEvent<RecipeUpdated, RecipeUpdatedBuilder>("any", 0, 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.UPDATE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_update_event_for_GroupUpdated()
        {
            var eventSetter = new Mock<IEventSetter<GroupUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<GroupUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<GroupUpdatedBuilder, string, int, int>(SetMandatoryFields_forUpdate);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateUpdateEvent<GroupUpdated, GroupUpdatedBuilder>("any", 0, 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.UPDATE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_update_event_for_MenuUpdated()
        {
            var eventSetter = new Mock<IEventSetter<MenuUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<MenuUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<MenuUpdatedBuilder, string, int, int>(SetMandatoryFields_forUpdate);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateUpdateEvent<MenuUpdated, MenuUpdatedBuilder>("any", 0, 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.UPDATE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_update_event_for_MealPeriodUpdated()
        {
            var eventSetter = new Mock<IEventSetter<MealPeriodUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<MealPeriodUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<MealPeriodUpdatedBuilder, string, int, int>((builder, cs, id1, id2) =>
                {
                    SetMandatoryFields_forUpdate(builder, cs, id1, id2);
                    SetMandatoryFields_forUpdate_forMealPeriod(builder, cs, id1, id2);
                });

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateUpdateEvent<MealPeriodUpdated, MealPeriodUpdatedBuilder>("any", 0, 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.UPDATE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_update_event_for_SupplierUpdated()
        {
            var eventSetter = new Mock<IEventSetter<SupplierUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<SupplierUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<SupplierUpdatedBuilder, string, int, int>(SetMandatoryFields_forUpdate);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var eventObject = eventFactory.CreateUpdateEvent<SupplierUpdated, SupplierUpdatedBuilder>("any", 0, 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.UPDATE, eventObject.ChangeType);
        }

        [Fact]
        public void Should_create_update_event_for_UserUpdated()
        {
            var eventSetter = new Mock<IEventSetter<UserUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<UserUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<UserUpdatedBuilder, string, int, int>(SetMandatoryFields_forUpdate);

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SupplierUpdatedBuilder>>(),
                eventSetter.Object);

            var eventObject = eventFactory.CreateUpdateEvent<UserUpdated, UserUpdatedBuilder>("any", 0, 0);

            Assert.NotNull(eventObject);
            Assert.True(eventObject.HasSequenceNumber);
            Assert.Equal(SourceSystem.STARCHEF, eventObject.Source);
            Assert.Equal(ChangeType.UPDATE, eventObject.ChangeType);
        }

        #endregion

        #region Mandatory fields setters

        private static void SetMandatoryFields_forUpdate(dynamic builder, string connectionString, int entityId, int databaseId)
        {
            builder.SetExternalId(entityId.ToString());
            builder.SetCustomerId(databaseId.ToString());
            builder.SetCustomerName(databaseId.ToString());
        }

        private static void SetMandatoryFields_forDelete(dynamic builder, string externalId, int databaseId)
        {
            builder.SetExternalId(externalId);
            builder.SetCustomerId(databaseId.ToString());
            builder.SetCustomerName(databaseId.ToString());
        }

        private static void SetMandatoryFields_forDelete_forMealPeriod(dynamic builder, string externalId, int databaseId)
        {
            builder.SetMealPeriodName("any");
        }

        private static void SetMandatoryFields_forUpdate_forMealPeriod(dynamic builder, string connectionString, int entityId, int databaseId)
        {
            builder.SetMealPeriodName("any");
        }

        #endregion
    }
}