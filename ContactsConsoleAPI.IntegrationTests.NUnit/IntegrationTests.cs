using ContactsConsoleAPI.Business;
using ContactsConsoleAPI.Business.Contracts;
using ContactsConsoleAPI.Data.Models;
using ContactsConsoleAPI.DataAccess;
using ContactsConsoleAPI.DataAccess.Contrackts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestContactDbContext dbContext;
        private IContactManager contactManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestContactDbContext();
            this.contactManager = new ContactManager(new ContactRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddContactAsync_ShouldAddNewContact()
        {
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

            var dbContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

            Assert.NotNull(dbContact);
            Assert.AreEqual(newContact.FirstName, dbContact.FirstName);
            Assert.AreEqual(newContact.LastName, dbContact.LastName);
            Assert.AreEqual(newContact.Phone, dbContact.Phone);
            Assert.AreEqual(newContact.Email, dbContact.Email);
            Assert.AreEqual(newContact.Address, dbContact.Address);
            Assert.AreEqual(newContact.Contact_ULID, dbContact.Contact_ULID);
        }

        //Negative test
        [Test]
        public async Task AddContactAsync_TryToAddContactWithInvalidCredentials_ShouldThrowException()
        {
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "invalid_Mail", //invalid email
                Gender = "Male",
                Phone = "0889933779"
            };

            var exeption = Assert.ThrowsAsync<ValidationException>(async () => await contactManager.AddAsync(newContact));
            var actual = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

            Assert.IsNull(actual);
            Assert.That(exeption.Message, Is.EqualTo("Invalid contact!"));

        }

        [Test]
        public async Task DeleteContactAsync_WithValidULID_ShouldRemoveContactFromDb()
        {
            // Arrange
            var firstContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(firstContact);
            // Act
            await contactManager.DeleteAsync(firstContact.Contact_ULID);
            // Assert
            var contactDB = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == firstContact.Contact_ULID);
            Assert.IsNull(contactDB);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
                public async Task DeleteContactAsync_TryToDeleteWithNullOrWhiteSpaceULID_ShouldThrowException(string InvalidCode)
        {

            // Act
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => contactManager.DeleteAsync(InvalidCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("ULID cannot be empty."));
        }

        [Test]
        public async Task GetAllAsync_WhenContactsExist_ShouldReturnAllContacts()
        {
            // Arrange
            var firstContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(firstContact);
            var secondContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(secondContact);
            // Act
            await contactManager.GetAllAsync();

            
            var result = await contactManager.GetAllAsync();
            // Assert
            Assert.IsNotNull(firstContact);
            Assert.IsNotNull(secondContact);
            Assert.That(result.Count, Is.EqualTo(2));

        }

        [Test]
        public async Task GetAllAsync_WhenNoContactsExist_ShouldThrowKeyNotFoundException()
        {



            // Act
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetAllAsync());
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("No contact found."));
        }
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task GetAllAsync_WhenNullProductCode_ShouldThrowArgumentException(string InvalidCode)
        {

            //Act
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => contactManager.GetSpecificAsync(InvalidCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("ULID cannot be empty."));
        }


        [Test]
        public async Task SearchByFirstNameAsync_WithExistingFirstName_ShouldReturnMatchingContacts()
        {
            // Arrange
            var firstContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(firstContact);
            // Act
            var result = await contactManager.SearchByFirstNameAsync(firstContact.FirstName);
            // Assert
            var resultContact = result.First();

            Assert.That(result, Is.Not.Null);
            Assert.That(resultContact.FirstName, Is.EqualTo(firstContact.FirstName));
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithNonExistingFirstName_ShouldThrowKeyNotFoundException()
        {

            // Act
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByFirstNameAsync("invalidName "));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("No contact found with the given first name."));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task SearchByFirstNameAsync_WithNullOrEmpryLastName_ShouldThrowArgumentException(string InvalidCode)
        {

            //Act
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => contactManager.SearchByFirstNameAsync(InvalidCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("First name cannot be empty."));
        }

        [Test]
        public async Task SearchByLastNameAsync_WithExistingLastName_ShouldReturnMatchingContacts()
        {
            // Arrange
            var firstContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(firstContact);
            // Act
            var result = await contactManager.SearchByLastNameAsync(firstContact.LastName);
            // Assert
            var resultContact = result.First();

            Assert.That(result, Is.Not.Null);
            Assert.That(resultContact.LastName, Is.EqualTo(firstContact.LastName));
        }

        [Test]
        public async Task SearchByLastNameAsync_WithNonExistingLastName_ShouldThrowKeyNotFoundException()
        {
            // Act
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByLastNameAsync("invalidName"));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("No contact found with the given last name."));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task SearchByLastNameAsync_WithNullOrEmpryLastName_ShouldThrowArgumentException(string InvalidCode)
        {

            //Act
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => contactManager.SearchByLastNameAsync(InvalidCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Last name cannot be empty."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidULID_ShouldReturnContact()
        {
            // Arrange
            var firstContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(firstContact);
            // Act
            var result = await contactManager.GetSpecificAsync(firstContact.Contact_ULID);
            // Assert
            Assert.That(result.Contact_ULID, Is.EqualTo(firstContact.Contact_ULID));
        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidULID_ShouldThrowKeyNotFoundException()
        {
            //Act
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetSpecificAsync("notCode"));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo($"No contact found with ULID: {"notCode"}"));

        }

        [Test]
        public async Task UpdateAsync_WithValidContact_ShouldUpdateContact()
        {
            // Arrange
            var firstContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(firstContact);
            // Act
            firstContact.Contact_ULID = "UPDATED123";
            await contactManager.UpdateAsync(firstContact);
            // Assert
            var result = await contactManager.GetSpecificAsync(firstContact.Contact_ULID);
            Assert.That(result.Contact_ULID, Is.EqualTo(firstContact.Contact_ULID));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidContact_ShouldThrowValidationException()
        {
            // Arrange
            var invalideContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "testgmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };
                       
            //Act
            var exeption = Assert.ThrowsAsync<ValidationException>(() => contactManager.UpdateAsync(invalideContact));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Invalid contact!"));
        }

        [Test]
        public async Task UpdateAsync_WithNullOrEmptyContact_ShouldThrowValidationException()
        {

            // Arrange
            //var invalideContact = null;
            //{
            //    FirstName = "TestFirstName",
            //    LastName = "TestLastName",
            //    Address = "Anything for testing address",
            //    Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
            //    Email = "testgmail.com",
            //    Gender = "Male",
            //    Phone = "0889933779"
            //};

            //Act
            var exeption = Assert.ThrowsAsync<ValidationException>(() => contactManager.UpdateAsync(null));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Invalid contact!"));
        }
    }
}
