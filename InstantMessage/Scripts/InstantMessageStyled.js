/*Test functions. 
*/

//generate a list of conversations in side bar upon click
$('#allCon').click(function () {
    getAllConversations();
})

$('#clear-messages').click(function () {
    $('#discussion').empty();
})

$('#sort').click(function () {
    console.log("sorting");
    conversationSort();

})
$('#display').click(function () {
    console.log("display");

    displayConversations();
})


$('#logOffButton').click(function () {
    CHAT.connection.stop();
    console.log("CHAT disconnected");
})

function testPrint() {
    console.log("All the Con ID's");
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {

        console.log(ALLCONVERSATIONS[i].conversationID);
    }
}


///Auxillary Functions///


function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

/*
Variables (Should be encapsulated in functions?)
*/
var RECIPIENTS = new Set();
var LIVECONVERSATIONID = null;
var CONVERSATIONNAME = null;
var CONNECTED = false;
var CHAT = $.connection.chatHub;
var ONSCREENCONVERSATION = null;
var USER = null;
var ALLCONTACTS = [];
var ALLCONVERSATIONS = [];

//////Models///////

function Conversation(con) {
    this.conversationID = con.ConversationID;
    this.name = con.Name;
    this.lastMessage = con.LastMessage;
    this.lastEdited = con.LastEdited;
    this.loaded = false;
    this.messages = [];
    this.users = [];
    this.addMessage = function (message) {
        this.messages.push(message);
    };
    this.unpackMessages = function () {
        return this.messages;
    };
    this.addUser = function (user) {
        this.users.push(user);
    };
    this.unpackUsers = function () {
        return this.users;
    };
}

function Message(message, user, conversationID) {
    this.content = message.Content;
    this.messageID = message.MessageID;
    this.sender = user;
    this.conversationID = conversationID;
    this.sent = message.Sent;
}

function Contact(user) {
    this.userID = user.UserID;
    this.userName = user.UserName;
    this.firstName = user.FirstName;
    this.lastName = user.LastName;
    this.lastActive = user.LastActive;
}


/*
Start Up and Reconnection 
*/
$.connection.hub.start()
    .done(
    $.connection.hub.logging = true,
    function () {
        console.log("success");
        CONNECTED = true;
        setConnectionStatus();
        getAllConversations();
    }).fail(function () {
        CONNECTED = false;
        setConnectionStatus();
    });

//attempt to reconnect 5 seconds after connection drops//refactor this with method above
//to reduce code duplication. 
$.connection.hub.disconnected(function () {
    setTimeout(function () {
        $.connection.hub.start().done(
            $.connection.hub.logging = true,
            function () {
                console.log("success");
                CONNECTED = true;
                setConnectionStatus();
               
            }).fail(function () {
                CONNECTED = false;
                setConnectionStatus();
                console.log("connection attempt failed");
            })
    }, 5000); // Restart connection after 5 seconds.
});




/*
Functions Called on Connection/Reconnection
*/

function setConnectionStatus() {
    if (CONNECTED == true) {
        $("#username").text("");
        $("#username").text(USER + " (Connected)");
    }
    else if (CONNECTED == false) {
        $("#username").text("");
        $("#username").text("No Connection");
    }
}

/*
   Conversation Side Bar
*/
CHAT.client.updateUser = function (user) {
    USER = user
    //add User variable here
    $("#username").text(USER);
}


//Requests all conversations from the server
function getAllConversations() {
    CHAT.server.getAllConversations();
}

//Conversation passed from server and added to array
CHAT.client.AddExistingConversation = function (c) {
    var con = new Conversation(c);
    ALLCONVERSATIONS.push(con);
}

//function called after all conversations received
CHAT.client.allConversationsAdded = function () {
    conversationSort();
    displayConversations();
}


//all conversations displayed on left panel
function displayConversations() {
    $("#conversation-header").empty();
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        var html = '<div class="conversation btn" id="' + ALLCONVERSATIONS[i].conversationID + '"><div class="media-body">' +
            '<h5 class="media-heading">' + ALLCONVERSATIONS[i].name + '</h5>' +
            '<small class="pull-left time">' + ALLCONVERSATIONS[i].lastMessage + '<br/></small>' +
            '<small class="pull-left time">' + ALLCONVERSATIONS[i].lastEdited + '<br/></small></div ></div >';
        $('#conversation-header').append(html);
    }
}

//Gets selected Conversation Id from the sidebar
$('#conversation-header').on('click','div',function (event) {
    var conId = $(this).attr('id');
    if (typeof conId !== "undefined")
    {
        accessConversation(conId);
    }
})

//Ensures that interface configured to display a conversation
function setDisplayForConversation(conId) {
    //set conversationId for any new messages. 
    LIVECONVERSATIONID = conId;
    ONSCREENCONVERSATION = conId
    $('#discussion').empty();
    $('#con-title-bar').text(" ");
    if (CONVERSATIONNAME !== null) {
        $('#con-title-bar').text(CONVERSATIONNAME); //MIGHT NOT WORK
    }
    else {
        var name = getConversationName(conId);
        if (name !== null) {
            $('#con-title-bar').text(name);
        }
    }
}

//Messages from a conversation requested from server
function loadConversationFromServer(conId) {
    CHAT.server.openConversation(conId);
}

//Function checks if a conversation is in cache, if not, request made to server 
function accessConversation(conID) {
    var loaded = checkIfLoaded(conID);
    if (loaded == true) {
        setDisplayForConversation(conID);
        displayCachedConversation(conID)
    }
    else {
        setDisplayForConversation(conID);
        loadConversationFromServer(conID);
    }
}

//displays a conversation from the cache
function displayCachedConversation(conversationID) {
    setDisplayForConversation(conversationID);
    var allMessages = returnConversationMessages(conversationID);
    for (var i = 0; i < allMessages.length; i++) {
        // Add the message to the page.
        $('#discussion').append('<div class="msg"><div class="media-body" >' +
            '<small class="pull-right time"><i class="fa fa-clock-o"></i>' + htmlEncode(allMessages[i].sent)+'</small>'+
            '<h5 class="media-heading">' + htmlEncode(allMessages[i].sender)+'</h5>'+
            '<small class="col-sm-11">' + htmlEncode(allMessages[i].content)+'</small></div ></div >');
    }
    displayParticipants(conversationID);
}

//displays all participants in a conversation
function displayParticipants(conversationID) {
    $('#members-panel').empty();
    var participants = getParticipants(conversationID);
    for (var i = 0; i < participants.length; i++)
    {
      html = '<div class="contact">' +
        '<div class="media-body" >' +
        '<h5 class="media-heading">'+participants[i].userID+'</h5>' +
          '<small class="pull-left time"><i>Last Active: ' + participants[i].lastActive+'</i></small>' +
            '</div></div>';
      $('#members-panel').append(html);
    }
}

//gets participants from the conversation object in the cache
function getParticipants(conversationID) { 
    var participants = [];
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            participants = ALLCONVERSATIONS[i].unpackUsers();
            return participants;
        }
    }
    return participants;
}

//Conversation participant passed from server and added to cache
CHAT.client.addConversationUser = function (user, conID) {
    var contact = new Contact(user);     
    for (var i = 0; i < ALLCONVERSATIONS.length; i++)
    {
        if (ALLCONVERSATIONS[i].conversationID == conID)
        {
            ALLCONVERSATIONS[i].addUser(contact);
        }
    }
}

/*
Displaying Conversations
*/

//Called once a conversation has been loaded to cache
CHAT.client.finishedLoadingConversation = function (conversationID) {
    setLoadedStatusTrue(conversationID);
    displayCachedConversation(conversationID);
}

//Passes conversation ID from server and configures screen 
CHAT.client.returnConversationDetails = function (conID) {
    LIVECONVERSATIONID = conID;
}

//checks is conversation is in a cache
function checkIfLoaded(conID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        //correct equality??
        if (ALLCONVERSATIONS[i].conversationID == conID) {

            if (ALLCONVERSATIONS[i].loaded === true) {

                return true;
            }
            else {

                return false;
            }
        }
    }
    return false;
}

//ensures that messages displayed and sent are displayed on screen
CHAT.client.setOnScreenConversation = function (conId) {
        ONSCREENCONVERSATION = conId;
}


//sort Conversations according to Last Edited.
function conversationSort() {
    ALLCONVERSATIONS.sort(function (a, b) {
        var conA = a.lastEdited.toUpperCase();
        var conB = b.lastEdited.toUpperCase();
        if (conA < conB) {
            return 1;
        }
        if (conA > conB) {
            return -1;
        }
        return 0;
    });
}


//message passed to the client and added to existing conversation
    CHAT.client.loadMessage = function (user, message, conversationID) {
        addMessageToConversation(message, user, conversationID);
    }

//Retrieves messages from a conversation to be displayed
function returnConversationMessages(conversationID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            return ALLCONVERSATIONS[i].unpackMessages();
        }
    }
}
//sets the loaded property of a conversation to be 'true'
function setLoadedStatusTrue(conversationID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            ALLCONVERSATIONS[i].loaded = true;
        }
    }
}

//adds a new message to an existing conversation
function addMessageToConversation(message, user, conversationID) {
    var cacheMessage = new Message(message, user, conversationID);
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == cacheMessage.conversationID) {
            ALLCONVERSATIONS[i].addMessage(cacheMessage);
        }
    }
}

/*
Displaying Contacts, Starting New Conversation
*/
//displays all contact - calls server if required - this does not account for new contacts being added to system!
$('#new-conversation').click(function () {
    //initialise recipients
    RECIPIENTS = new Set();
    //$(".contact btn").remove();
    $('#contact-block').text("");
    if (ALLCONTACTS.length < 1) {
        CHAT.server.getContacts();
    }
    else {
        displayContacts();
    }
})

//receives contact from the server and adds to array of contacts
CHAT.client.passContact = function (user) {
    var contact = new Contact(user);
    ALLCONTACTS.push(contact);
}

//Server informs client that all contacts have been loaded
CHAT.client.ShowContacts = function () {
    displayContacts();
}

//displays all contacts
function displayContacts() {
    $("#contact-panel").empty();
    for (var i = 0; i < ALLCONTACTS.length; i++) {
        var html = '<div class="contact btn" id="' + ALLCONTACTS[i].userID + '">' +
            '<div class="media-body">' +
            '<h5 class="media-heading">' + ALLCONTACTS[i].userID + '</h5>' +
            '</div></div>';
        $('#contact-panel').append(html);
    }
}

//Gets contactId from nav side bar and adds to the Recipients array
$('#contact-panel').on('click', 'div', function (event) {
    $('#discussion').empty();
    LIVECONVERSATIONID = null;
    var contactId = $(this).attr('id');
    console.log("contact id is " +contactId);
    if (typeof contactId !== "undefined")
    {
        RECIPIENTS.add(contactId);

        if (RECIPIENTS.size < 2)
        {
            $('#con-title-bar').text(contactId + " ")
        }

        if (RECIPIENTS.size == 2)
        {
            $('#con-title-bar').text(" ");
            $('#myModal').modal('show');
        }

        if (RECIPIENTS.size > 2)
        {
            $('#con-title-bar').text(CONVERSATIONNAME + ' (' + displayRecipients() + ')');

        }
    }
})


$("#conNameButton").click(function () {
    $('#con-title-bar').text(" ");
    CONVERSATIONNAME = $('#conName').val();
    $('#con-title-bar').text(CONVERSATIONNAME + ' (' + displayRecipients() + ')');

})

function displayRecipients() {
    var recipients = "";
    RECIPIENTS.forEach(function (value) {
        recipients += value + ", ";   
    });
    return recipients;
}

    $('#send-message').click(function () {
        var message = $('#message').val();
        if (message.length > 0 && LIVECONVERSATIONID === null) {
          //  var recipientArray = [];
            recipientArray = Array.from(RECIPIENTS);
            if (recipientArray.length > 0)
            {
                CHAT.server.sendFirstMessage(message, recipientArray, CONVERSATIONNAME);
                RECIPIENTS = new Set();
                CONVERSATIONNAME = null;
                // Clear text box and reset focus for next message.
                $('#message').val('').focus();
            }
        }
        else if (message.length > 0 && LIVECONVERSATIONID !== null) {
            CHAT.server.send(message, LIVECONVERSATIONID);
            $('#message').val('').focus();
        }
    })

CHAT.client.receiveNewConversation = function (con) {
    var newCon = new Conversation(con);
    CHAT.server.joinGroupRemotely(newCon.conversationID);
    ALLCONVERSATIONS.push(newCon);
    // newMessageHandler(message, user, con);
    conversationSort();
    displayConversations();

    }


    CHAT.client.updateExistingConversation = function (con) {
        for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
            if (ALLCONVERSATIONS[i].conversationID == con.ConversationID) {
                ALLCONVERSATIONS[i].lastEdited = con.LastEdited;
                ALLCONVERSATIONS[i].lastMessage = con.LastMessage;
            }
        }
        conversationSort();
        displayConversations();
    }


    CHAT.client.messageHandler = function (message, user, conID) {
        console.log("in the message handler");
        var loaded = checkIfLoaded(conID);
        console.log("loaded = " + loaded);
        if (loaded == true) {
            console.log("message handler , loaded == true");
            var cacheMessage = addNewMessageToConversation(message, user, conID);

            if (ONSCREENCONVERSATION == cacheMessage.conversationID) {
                displayCachedConversation(cacheMessage.conversationID)
            }
            else {
                //do nothing
            }
        }
        else if (loaded == false) {
            if (ONSCREENCONVERSATION == conID) {
                loadConversationFromServer(conID);
            }
            else {
                //do nothing
            }
        }
    }

function getConversationName(conversationID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            return ALLCONVERSATIONS[i].name;
        }
    }
}

function addNewMessageToConversation(message, user, conversationID) {
    var cacheMessage = new Message(message, user, conversationID);
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == cacheMessage.conversationID) {
            ALLCONVERSATIONS[i].addMessage(cacheMessage);
            // ALLCONVERSATIONS[i].lastMessage = cacheMessage.content;
            // ALLCONVERSATIONS[i].lastEdited = cacheMessage.sent;
        }
    }
    return cacheMessage;
}

