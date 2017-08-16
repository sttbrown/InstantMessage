///Auxillary Functions///

function makeConvDivInvisible() {
    var conDiv = document.getElementById('conversation-container');
    var content = document.getElementById('conversation-content');
    var message = document.getElementById('message-div');
    content.style.display = 'none';
    message.style.display = 'none';
    conDiv.style.display = 'none';
}

function makeConvDivVisible() {
    var conDiv = document.getElementById('conversation-container');
    var content = document.getElementById('conversation-content');
    var message = document.getElementById('message-div');
    content.style.display = 'block';
    message.style.display = 'block';
    conDiv.style.display = 'block';
}

function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}


/* Set the width of the side navigation to 250px and the left margin of the page content to 250px */
function openNav() {
    document.getElementById("mySideNav").style.width = "350px";
    document.getElementById("main").style.marginLeft = "350px";

}

//for development purposes leave nav open on load
$(function () {
    openNav();
})

function openNewConversationNav() {
    document.getElementById("newConversationSideNav").style.width = "350px";
    document.getElementById("main").style.marginLeft = "350px";
}

/*NOT IN USE Set the width of the side navigation to 0 and the left margin of the page content to 0 */
function closeNav() {
    document.getElementById("mySideNav").style.width = "0";
    document.getElementById("main").style.marginLeft = "250px";
}

function closeNewConversationNav() {
    document.getElementById("newConversationSideNav").style.width = "0";
    document.getElementById("main").style.marginLeft = "350px";
}

/*
Global Variables (Should be encapsulated in functions?)
*/
var RECIPIENTS = [];
var LIVECONVERSATIONID = null;
var CONVERSATIONNAME = null;
var CONNECTED = false;
var CHAT = $.connection.chatHub;
var ONSCREENCONVERSATION = null;

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
    this.addMessage = function (message) {
        this.messages.push(message);
    }
    this.unpackMessages = function () {
        return this.messages;
    }
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
    this.LastName = user.LastName;
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



$('#logOffButton').click(function () {
    CHAT.connection.stop();
    console.log("CHAT disconnected");
})

/*
Functions Called on Connection/Reconnection
*/

function getAllConversations() {
    $(".conItem").remove();
    CHAT.server.getAllConversations();
}

function setConnectionStatus() {
    if (CONNECTED == true) {
        $("#connectionSpan").text("");
        $("#connectionSpan").text("Connection Status: Connected!");
    }
    else if (CONNECTED == false) {
        $("#connectionSpan").text("");
        $("#connectionSpan").text("Connection Status: No Connection");
    }
}

/*
    Loading Conversation Side Bar
*/

//generate a list of conversations in side bar upon click
$('#existingConversationNavSpan').click(function () {
    $(".conItem").remove();
    getAllConversations();
})

/*
Displaying Conversations
*/
CHAT.client.returnConversationDetails = function (conID) {
    LIVECONVERSATIONID = conID;
    //maybe set ONSCREENCONVERSATION here as well?
}

$('#mySideNav').on('click', 'a', function (event) {
    var conId = $(this).attr('id');
    var conName = $(this).attr('data-address');
    accessConversation(conId, conName);
})




function checkIfLoaded(conID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        //correct equality??
        if (ALLCONVERSATIONS[i].conversationID == conID) {
            if (ALLCONVERSATIONS[i].loaded === true) {
                return true;
            }
        }
    }
    return false;
}

$(function () {
    CHAT.client.setOnScreenConversation = function (conId) {
        console.log(" CHAT.client.setOnScreenConversation --" + conId);
        ONSCREENCONVERSATION = conId;
        console.log("On screen conversations set = " + ONSCREENCONVERSATION);

    }
})


// $(function () {
//   CHAT.client.updateConversations = function () {
//     conversationSort();
//   displayConversations();
// }
// })
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

/*
Loading, Displaying, Sending Messages
*/


//load message function
$(function () {
    CHAT.client.loadMessage = function (user, message, conversationID) {
        addMessageToConversation(message, user, conversationID);
    }
})

function returnConversationMessages(conversationID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            return ALLCONVERSATIONS[i].unpackMessages();
        }
    }
}

function setLoadedStatusTrue(conversationID) {

    console.log("set loaded status true");
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            console.log("con loaded = true in 'setLoadedStatusTrue' method");
            ALLCONVERSATIONS[i].loaded = true;
        }
    }
}

CHAT.client.finishedLoadingConversation = function (conversationID) {
    setLoadedStatusTrue(conversationID);
    displayCachedConversation(conversationID);
}



function displayConversations() {
    $(".conItem").remove();
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        var html = '<div class="conitembox"><a href="javascript:void(0)" class="conItem" id="' + ALLCONVERSATIONS[i].conversationID + '", data-address="' + ALLCONVERSATIONS[i].name + '">' + ALLCONVERSATIONS[i].name + '</a><br/>' +
            '<p class="condetails"><strong>' + ALLCONVERSATIONS[i].lastMessage + '</strong></br> ' + ALLCONVERSATIONS[i].lastEdited + '</p><br/></div>';
        $('#mySideNav').append(html);
    }
}

//Generate list of conversations on side bar
$(function () {
    CHAT.client.AddExistingConversation = function (c) {
        var con = new Conversation(c);
        ALLCONVERSATIONS.push(con);
    }
})


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
$('#newConversationNavSpan').click(function () {
    //initialise recipients
    RECIPIENTS = [];
    $(".contactItem").remove();
    $('#contact-block').text("");
    if (ALLCONTACTS.length < 1) {
        CHAT.server.getContacts();
    }
    else {
        displayContacts();
    }
})

CHAT.client.passContact = function (user) {
    var contact = new Contact(user);
    ALLCONTACTS.push(contact);
}

CHAT.client.ShowContacts = function () {
    displayContacts();
}

function displayContacts() {
    $(".contactItem").remove();
    for (var i = 0; i < ALLCONTACTS.length; i++) {
        html = '<a href="javascript:void(0)" class="contactItem" id=' + ALLCONTACTS[i].userID + '>' + ALLCONTACTS[i].userID + '<br/></a>';
        $('#newConversationSideNav').append(html);
    }
}

//Gets contactId from nav side bar
$('#newConversationSideNav').on('click', 'a', function (event) {
    var contactId = $(this).attr('id');
    console.log(contactId);
    if (contactId !== undefined) {
        $('#contact-block').append(contactId + " ")
        RECIPIENTS.push(contactId);
    }
    if (RECIPIENTS.length == 2) {
        $('#myModal').modal('show');
    }
})

$("#conNameButton").click(function () {
    CONVERSATIONNAME = $('#conName').val();
})


$('#startNewButton').click(function () {
    var conId = null; //undefined
    setDisplayForConversation(conId);
    closeNewConversationNav();
})

$(function () {
    $('#sendmessage').click(function () {
        var message = $('#message').val();
        if (message.length > 0 && LIVECONVERSATIONID === null) {
            CHAT.server.sendFirstMessage(message, RECIPIENTS, CONVERSATIONNAME);
            // Clear text box and reset focus for next message.
            $('#message').val('').focus();
            RECIPIENTS = [];
            CONVERSATIONNAME = null;
        }
        else if (message.length > 0 && LIVECONVERSATIONID !== null) {
            CHAT.server.send(message, LIVECONVERSATIONID);
            $('#message').val('').focus();
        }
    })
})

/////////
CHAT.client.allConversationsAdded = function () {
    displayConversations();
}


//Add new sent message to page
CHAT.client.transferMessage = function (message, user, conID) {
    newMessageHandler(message, user, conID);
    conversationSort();
    displayConversations();
}
//IS THIS USED ANYMORE>>>>????
function newMessageHandler(message, user, conID) {
    var cacheMessage = addNewMessageToConversation(message, user, conID);
    // Add the message to the page if user has this conversation on screen
    if (ONSCREENCONVERSATION == cacheMessage.conversationID) {
        $('#discussion').append('<li class="chatItem"><strong>' + htmlEncode(cacheMessage.sender)
            + '</strong>: ' + htmlEncode(cacheMessage.content) + '</li>');
    }
    else {
        console.log("client does not have this conversation loaded");
    }
}

//Part 1, condition 1
CHAT.client.receiveNewConversation = function (con) {
    var newCon = new Conversation(con);
    CHAT.server.joinGroupRemotely(newCon.conversationID);
    ALLCONVERSATIONS.push(newCon);
    // newMessageHandler(message, user, con);
    conversationSort();
    displayConversations();

}

$(function () {
    CHAT.client.newMessageNotification = function (conId) {
        // html = '<a href="javascript:void(0)" id="' + conId + '" onclick="goToMessage()">click here to view conversation</a><br/>';
        $('#newMessageNotificationSpan').text("You have a new message");
        //do something with this.

    }
})

$(function () {
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
})

$(function () {
    CHAT.client.messageHandler = function (message, user, conID) {
        var loaded = checkIfLoaded(conID);

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
})

function getConversationName(conversationID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            return ALLCONVERSATIONS[i].name;
        }
    }
}


function displayCachedConversation(conversationID) {
    makeConvDivVisible();
    setDisplayForConversation(conversationID);
    var allMessages = returnConversationMessages(conversationID);
    for (var i = 0; i < allMessages.length; i++) {
        // Add the message to the page.
        $('#discussion').append('<li class="chatItem"><strong>' + htmlEncode(allMessages[i].sender)
            + '</strong>: ' + htmlEncode(allMessages[i].content) + '</li></br>');
    }
}


function addNewMessageToConversation(message, user, conversationID) {
    var cacheMessage = new Message(message, user, conversationID);
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == cacheMessage.conversationID) {
            ALLCONVERSATIONS[i].addMessage(cacheMessage);
            // ALLCONVERSATIONS[i].lastMessage = cacheMessage.content;
            //  ALLCONVERSATIONS[i].lastEdited = cacheMessage.sent;
        }
    }
    return cacheMessage;
}

//this can be used whether or not the conversation has already been loaded..
function setDisplayForConversation(conId) {
    //set conversationId for any new messages. 
    LIVECONVERSATIONID = conId;
    ONSCREENCONVERSATION = conId
    //reset notifications since user has accessed sideNav
    $('#newMessageNotificationSpan').text("Notifications:");
    $('#discussion').empty();
    $('#conversationHeading').text("");
    if (CONVERSATIONNAME !== null) {
        $('#conversationHeading').text(CONVERSATIONNAME);
    }
    else {
        var name = getConversationName(conId);

        if (name !== null) {
            $('#conversationHeading').text(name);
        }
    }
}

function loadConversationFromServer(conId) {
    CHAT.server.openConversation(conId);
}


function accessConversation(conID, conName) {
    var loaded = checkIfLoaded(conID);
    if (loaded == true) {
        setDisplayForConversation(conID);
        displayCachedConversation(conID)
        console.log("loaded boolean is true");
    }
    else {
        setDisplayForConversation(conID);
        loadConversationFromServer(conID);
        console.log("loaded boolean is false");
    }
}