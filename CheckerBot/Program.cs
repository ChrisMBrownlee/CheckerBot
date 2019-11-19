using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CheckerBot {

    class Program {
        #region GLOBALS
        //GLOBAL BOARD DATA CONTROLS
        static int Size = 8;
        static int ControlStart = 5;
        static int TextPosX = 0;
        static int TextPosY = 33;
        static int SquarePosWidth = 8;
        static int SquarePosHeight = 4;
        static System.ConsoleColor OG_BG_Color = Console.BackgroundColor;
        static System.ConsoleColor OG_FG_Color = Console.ForegroundColor;
        static System.ConsoleColor BoardColor1 = Console.ForegroundColor;
        static System.ConsoleColor BoardColor2 = Console.ForegroundColor;
        static System.ConsoleColor Player1Color = Console.ForegroundColor;
        static System.ConsoleColor Player2Color = Console.ForegroundColor;

        //GLOBAL ASCII CHARACTER
        static char Block = (char)9608;
        static char GamePiece = (char)9679;
        
        //GLOBAL 2D ARRAY
        static System.ConsoleColor[,] BoardSlotColor = new ConsoleColor[Size, Size];
        static string[,] Coords = new string[Size, Size];
        static bool[,] HasPlayer1 = new bool[Size, Size];
        static bool[,] HasPlayer2 = new bool[Size, Size];
        static string[,] ManOrKing = new string[Size, Size];
        static bool[,] CanPlace = new bool[Size, Size];

        //GLOBAL COORDINATE DATA
        static int[] StoredCoords = new int[4];
        static int[] BoardStoredCoords = new int[2];
        static string[,] SquareCursorPos = new string[Size, Size];

        //GLOBAL JUMP DATA
        static bool DidJump = false;
        static string JumpedPiece = "";

        //GLOBAL PLAYERS DATA
        static int PlayerTurn = 1;
        static int Player1DeadMen = 0;
        static int Player2DeadMen = 0;
        #endregion

        static void Main(string[] args) {//START MAIN
            Console.SetWindowSize(200, 58);
            Console.Title = "CheckerBot v1.0 by Chris Brownlee";
            //VARIABLES
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool playagain = false;

            GetDrawPosFunc();

            do {
                //STARTUP VARIABLES || RESET VARIABLES ON PLAY AGAIN
                bool won = false;
                bool valid = true;
                Player1DeadMen = 0;
                Player2DeadMen = 0;
                PlayerTurn = 1;

                //FUNCTION TO DRAW THE BOARD THAT USES THE DRAW SQUARE FUNCTION AND DRAW CHECKER FUNCTION
                DrawBoardFunc(Size - 1, Size - 1);

                //PLAY THE GAME
                do {//THIS WHILE GAME IS PLAYING
                    DidJump = false;

                    do {//WHILE THE POSITIONS ARE NOT VALID
                        //STEP 1 - GET PLAYER 1 PIECE POS -> POS || GET PLAYER 2 PIECE POS -> POS                           
                        PromptPlayer();

                        //STEP 2 - CHECK IF ILLEGAL OR LEGAL MOVEMENT
                        valid = IllegalMoveCheckFunc();

                    } while(!valid);

                    //STEP 3 - IF LEGAL CHECK IF PIECE GOT KING'D
                    KingStatusFunc();

                    //STEP 4 - IF LEGAL MOVEMENT THEN MOVE PIECE
                    MovePieceFunc();
                    if(DidJump) {
                        JumpPieceRemovalFunc();
                    }//end if  

                    //STEP 5.1 - CHECK IF PLAYER 1 HAS ALL PIECES || IF PLAYER 2 HAS ALL PIECES
                    //STEP 5.2 - IF PLAYER HAS 0 PIECES MAKE PLAYER# WINNER
                    DetermineWinnerFunc();


                    //STEP 6 - CHANGE PLAYER 1 -> PLAYER 2 || PLAYER 2 -> PLAYER 1
                    PlayerTurn = PlayerWhatFunction(PlayerTurn);                 

                } while(!won);

                playagain = PlayAgainFunc();
            } while(playagain);
        }//end main

        //START UP FUNCTIONS
        #region GET SQUARE STARTING DRAW POSITIONS FUNCTION
        static void GetDrawPosFunc() {
            //VARS
            int countX = (-SquarePosWidth);
            int countY = 0;

            //LOOPS TO GET EACH BOARD SQUARE'S STARTING LOCATION
            for(int index = 0; index < Size; index++) {
                for(int index2 = 0; index2 < Size; index2++) {
                    countX += SquarePosWidth;

                    SquareCursorPos[index2, index] = $"{countX}, {countY}";

                    Coords[index2, index] = $"{index2}, {index}";
                }//end for
                //UP Y POSITION AND RESET X POSITION
                countY += SquarePosHeight;
                countX = (-SquarePosWidth);
            }//end for
        }//end region
        #endregion

        //BOARD SETUP FUNCTIONS
        #region DRAW SQUARE FUNCTION
        static void DrawSquareFunc(string XandY, System.ConsoleColor color) {//DRAW A SINGLE SQUARE AT ANY POSITION AND ANY COLOR
            //CHANGE BACKBROUND COLOR
            Console.BackgroundColor = color;
            Console.ForegroundColor = color;            

            //CONVERT NUMBER
            ConvertBoardSlot(XandY);

            //PLACE BOARD TILE
            Console.SetCursorPosition(BoardStoredCoords[0], BoardStoredCoords[1]);

            //LOOPS TO WRITE THE BOARD SQUARE PATTERN
            for(int index = 0; index < SquarePosHeight; index++) {
                for(int index2 = 0; index2 < SquarePosWidth; index2++) {
                    Console.Write(Block);
                }//end for
                //MOVES CURSER POSITION ON Y AXIS DOWN BY 1
                Console.SetCursorPosition(BoardStoredCoords[0], BoardStoredCoords[1] + (index + 1));
            }//end for

            //RESET COLOR
            Console.ResetColor();

        }//end function
        #endregion

        #region DRAW CHECKER FUNCTION
        static void DrawCheckerFunc(string XandY, System.ConsoleColor color) {//FUNCTION TO DRAW A CHECKER AT ANY POSITION AND ANY COLOR
            //VARS
            int HalfWidth = SquarePosWidth / 2;
            int HalfHeight = SquarePosHeight /2;

            //CHANGE TOKEN COLOR
            Console.BackgroundColor = color;
            Console.ForegroundColor = color;

            //CONVERT NUMBER
            ConvertBoardSlot(XandY);

            //PLACE TOKEN ON BOARD
            Console.SetCursorPosition((BoardStoredCoords[0] + (HalfWidth / 2)), (BoardStoredCoords[1] + (HalfHeight / 2)));

            //LOOP TO WRITE IN WHERE GAMEPIECE GOES
            for(int index = 0; index < HalfHeight; index++) {
                for(int index2 = 0; index2 < HalfWidth; index2++) {
                    Console.Write(GamePiece);
                }//end for
                Console.SetCursorPosition((BoardStoredCoords[0] + (HalfWidth / 2)), (BoardStoredCoords[1] + ((HalfHeight / 2 ) + (index + 1))));
            }//end for

            //IF GAMEPIECE IS KING LOOP TO WRITE "KING" ABOVE GAMEPIECE
            if(ManOrKing[StoredCoords[0], StoredCoords[1]] == "king") {
                Console.SetCursorPosition((BoardStoredCoords[0] + (HalfWidth / 2)), (BoardStoredCoords[1] + (HalfHeight / 2)) - 1);
                Console.BackgroundColor = BoardColor2;
                if(PlayerTurn == 1) {//PLAYER 1 SETS TEXT TO PLAYER 1 COLOR
                    Console.ForegroundColor = Player1Color;
                } else if (PlayerTurn == 2) {//PLAYER 2 SETS TEXT TO PLAYER 2 COLOR
                    Console.ForegroundColor = Player2Color;
                }//end if
                Console.Write("KING");
            }//end if
        }//end function
        #endregion
        
        #region DRAW BOARD FUNCTION
        static void DrawBoardFunc(int size_x, int size_y) {
            //VARIABLES            
            int start_x = ControlStart;
            int start_y = ControlStart;
            int y_count = 0;
            bool flipflop = true;
            
            //GET COLORS FOR BOARD
            BoardColor1 = ConsoleColor.Red; 
            BoardColor2 = ConsoleColor.Blue;            

            //LOOP FOR MAKING BOARD
            for(int index = 0; index <= size_y; index++) {                
                Console.Write(y_count);
                y_count++;                

                for(int index2 = 0; index2 <= size_x; index2++) {//RUN THROUGH X POS
                    if(flipflop) {
                        DrawSquareFunc(SquareCursorPos[index2, index], BoardColor1);//SET BOARD COLOR TO FIRST COLOR
                        
                        //STORE COLOR INTO COLOR SLOT
                        BoardSlotColor[index2, index] = BoardColor1;

                        if(index2 != size_x) {
                            flipflop = false;
                        }//end if                        
                    } else {
                        DrawSquareFunc(SquareCursorPos[index2, index], BoardColor2);//SET BOARD COLOR TO SECOND COLOR

                        //STORE COLOR INTO COLOR SLOT
                        BoardSlotColor[index2, index] = BoardColor2;

                        if(index2 != size_x) {
                            flipflop = true;
                        }//end if
                    }//end if                    
                }//end for
                //UP Y POS AND RESET X TO START POS THEN MOVE CURSOR
                start_y++;
                start_x = ControlStart;                 
                //Console.SetCursorPosition(start_x, start_y);
            }//end for

            //GET COLORS FOR PLAYERS
            Player1Color = ConsoleColor.White;
            Player2Color = ConsoleColor.Black;

            //RESET NUMBERS FOR COUNTING
            start_x = ControlStart;
            start_y = ControlStart + 1;
            y_count = 0;

            //LOOP FOR MAKING PLAYER PIECES
            for(int index = 0; index  <= size_y; index++) {//Y LOOP
                for(int index2 = 0; index2 <= size_x; index2++) {//X LOOP
                    if(BoardSlotColor[index2, index] == BoardColor2 && y_count == 0 || //P1 STARTING SPOTS
                       BoardSlotColor[index2, index] == BoardColor2 && y_count == 1 || //...
                       BoardSlotColor[index2, index] == BoardColor2 && y_count == 2) { //...
                        DrawCheckerFunc(SquareCursorPos[index2, index], Player1Color);
                        HasPlayer1[index2, index] = true;   //TELLS THAT THIS IS PLAYER 1 PIECE
                        HasPlayer2[index2, index] = false;  //TELLS THAT THIS IS NOT PLAYER 2 PIECE
                        ManOrKing[index2, index] = "man";   //SETUP FOR START OF GAME
                        CanPlace[index2, index] = true;     //SETUP FOR BOARD SPACES YOU CAN PLACE A PIECE ON
                    } else if (BoardSlotColor[index2, index] == BoardColor2 && y_count == size_x ||         //P2 STARTING SPOTS
                               BoardSlotColor[index2, index] == BoardColor2 && y_count == (size_x - 1) ||   //...
                               BoardSlotColor[index2, index] == BoardColor2 && y_count == (size_x - 2)) {   //...
                        DrawCheckerFunc(SquareCursorPos[index2, index], Player2Color);
                        HasPlayer2[index2, index] = true;   //TELLS THAT THIS IS PLAYER 2 PIECE
                        HasPlayer1[index2, index] = false;  //TELLS THAT THIS IS NOT PLAYER 1 PIECE;
                        ManOrKing[index2, index] = "man";   //SETUP FOR START OF GAME
                        CanPlace[index2, index] = true;     //SETUP FOR BOARD SPACES YOU CAN PLACE A PIECE ON
                    } else {//BLANK STARTING SPOTS
                        start_x++;
                        HasPlayer1[index2, index] = false;  //TELLS THAT THIS IS NOT PLAYER 1 PIECE
                        HasPlayer2[index2, index] = false;  //TELLS THAT THIS IS NOT PLAYER 2 PIECE
                        ManOrKing[index2, index] = "none";  //SETUP FOR START OF GAME
                        CanPlace[index2, index] = false;    //SETUP FOR BOARD SPACES YOU CAN PLACE A PIECE ON
                    }//end if
                }//end for

                //COUNT UP FOR KNOWING Y POS
                y_count++;
                //UP START Y POS AND RESET X TO START POS THEN MOVE CURSOR
                start_y++;
                start_x = ControlStart;
                Console.SetCursorPosition(start_x, start_y);
            }//end for

        }//end function
        #endregion

        //PLAY GAME FUNCTIONS
        //STEP 1
        #region GET PLAYER MOVEMENT
        static void PromptPlayer() {            
            //VARS            
            bool valid = false;
            string answer = "";

            //GATHER PLAYER'S COORDS FROM 'HERE' TO 'THERE'
            do {
                SetupTextFunc();
                //ASK PLAYER WHICH PIECE TO MOVE AND READ ANSWER
                Console.Write($"Player {PlayerTurn}, which piece do you want to move and to where? (Ex: 12-34 is (1,2) to (3,4)): ");
                answer = Console.ReadLine();
                
                //CHECK IF ANSWER CONTAINS NUMBERS AND HYPHEN AT MOST
                valid = StringIsNumeric(answer);

                if(valid) {
                    //CONVERT THE ANSWER INTO INT ARRAY
                    ConvertPositions(answer);

                    //CHECK IF VALID  OR NOT
                    valid = ValidCoordsFunc();
                }//end if

                if(!valid) {//NOT VALID SEND ERROR
                    SetupTextFunc();
                    Console.WriteLine($"Player {PlayerTurn}, you entered bad coordinates. Try again (Press any key to continue).");
                    Console.ReadKey();
                }//end if

            } while(!valid);
                        
        }//end function
        #endregion        

        #region IS VALID COORDS FUNCTION
        static bool ValidCoordsFunc() {
            //VARS
            bool valid = false;
            int count = 0;

            //LOOP TO CHECK IF COORDS GIVEN ARE WITHING 0 <-> 8
            for(int index = 0; index < 4; index++) {
                for(int index2 = 0; index2 < 8; index2++) {
                    if(StoredCoords[index] == index2) {
                        count++;//THIS COUNTS HOW MANY TIMES IT MATCHED
                    }//end if
                }//end for
            }//end for

            if(count == 4) {//IF ALL 4 MATCHED THEN IT'S VALID
                valid = true;
            }//end if

            return valid;
        }//end function
        #endregion

        //STEP 2
        #region ILLEGAL MOVE CHECKER FUNCTION
        static bool IllegalMoveCheckFunc() {
            bool valid = true;

            if(ManOrKing[StoredCoords[0], StoredCoords[1]] == "none") {//CAN'T MOVE A PIECE THAT'S NOT THERE
                valid = false;
            } else {//CHECK OTHER FACTORS OF MOVEMENT
                valid = ManPieceMoveCheck();
            }//end if

            if(!valid) {//NOT VALID SEND ERROR
                SetupTextFunc();
                Console.WriteLine("You're trying to make an illegal move. Try again. (Press any key to continue)");
                Console.ReadKey();
            }//end if

            return valid;
        }//end function
        #endregion

        #region GAMEPIECE ILLEGAL MOVE CHECK FUNCTION
        static bool ManPieceMoveCheck() {
            bool valid = true;

            //GET X AND Y COORDS
            int answerX = StoredCoords[0] - StoredCoords[2];
            int answerY = StoredCoords[1] - StoredCoords[3];

            //CHECK IF PLAYER GIVES MOVE THAT IS FURTHER THAN 2 SPACES AWAY
            if(answerX > 2 || answerX < -2) {//CHECK X POS
                return false;
            } else if (answerY > 2 || answerY < -2) {//CHECK Y POS
                return false;
            }//end if

            //CHECKS FOR VALID MOVEMENTS
            if((StoredCoords[1] + 2) == StoredCoords[3] && (StoredCoords[0] + 2) == StoredCoords[2] ||//PLAYER TRYING TO JUMP
               (StoredCoords[1] + 2) == StoredCoords[3] && (StoredCoords[0] - 2) == StoredCoords[2] ||//...
               (StoredCoords[1] - 2) == StoredCoords[3] && (StoredCoords[0] + 2) == StoredCoords[2] ||//...
               (StoredCoords[1] - 2) == StoredCoords[3] && (StoredCoords[0] - 2) == StoredCoords[2]) {//...
                valid = JumpMoveCheck();
            } else {//NORMAL ONE SPACE MOVEMENT
                valid = OneSpaceMoveCheck();
            }//end if           

            return valid;
        }//end function
        #endregion

        #region ONE SPACE MOVEMENT CHECKER FUNCTION
        static bool OneSpaceMoveCheck() {
            if(ManOrKing[StoredCoords[0],StoredCoords[1]] == "man") {//CHECK FOR IF MAN (ManOrKing)
                if(PlayerTurn == 1) {//PLAYER 1
                    if(StoredCoords[2] != (StoredCoords[0] + 1) && StoredCoords[2] != (StoredCoords[0] - 1)) {//CAN ONLY MOVE ONE SPACE (LEFT OR RIGHT) IN X POS
                        return false;
                    } else if(StoredCoords[3] != (StoredCoords[1] + 1)) { //CAN ONLY MOVE POSITIVELY IN Y POS
                        return false;
                    }//end if
                } else { //PLAYER 2
                    if(StoredCoords[2] != (StoredCoords[0] + 1) && StoredCoords[2] != (StoredCoords[0] - 1)) {//CAN ONLY MOVE ONE SPACE (LEFT OR RIGHT) IN X POS
                        return false;
                    } else if(StoredCoords[3] != (StoredCoords[1] - 1)) {//CAN ONLY MOVE NEGATIVELY IN Y POS
                        return false;
                    }//end if
                }//end if
            } else if (ManOrKing[StoredCoords[0], StoredCoords[1]] == "king") {//CHECKS FOR IF KING (ManOrKing)
                if(StoredCoords[2] != StoredCoords[0] + 1 && StoredCoords[2] != StoredCoords[0] - 1 && //KING MOVES PLUS ONE OR MINUS ONE X POS
                   StoredCoords[3] != StoredCoords[1] + 1 && StoredCoords[3] != StoredCoords[1] - 1) { //KING MOVES PLUS ONE OR MINUS ONE Y POS
                    return false;
                }//end if
            } else {//CHECKS FOR IF NONE (ManOrKing)
                return false;
            }//end if
            return true;
        }//end function
        #endregion 

        #region JUMP PIECE MOVEMENT CHECKER FUNCTION
        static bool JumpMoveCheck() {
            int answerX;
            int answerY;

            if(ManOrKing[StoredCoords[0], StoredCoords[1]] == "man") {//CHECKS IF MAN (ManOrKing)
                if(PlayerTurn == 1) {//PLAYER 1 MAN MOVEMENT CHECK
                    if(StoredCoords[1] < StoredCoords[3]) {//PLAYER 1 MAN MUST MOVE POSITIVELY IN Y POS
                        if(StoredCoords[0] < StoredCoords[2]) {//X POS MOVES RIGHT
                            answerX = StoredCoords[0] + 1; //GET JUMPED PIECE POS X
                            answerY = StoredCoords[1] + 1; //GET JUMPED PIECE POS Y

                            //STORE JUMPED POS
                            JumpedPiece = $"{answerX}, {answerY}";

                            if(HasPlayer1[answerX, answerY] == true || HasPlayer2[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        } else {//X POS MOVES LEFT
                            answerX = StoredCoords[0] - 1; //GET JUMPED PIECE POS X
                            answerY = StoredCoords[1] + 1; //GET JUMPED PIECE POS Y

                            //STORE JUMPED POS
                            JumpedPiece = $"{answerX}, {answerY}";

                            if(HasPlayer1[answerX, answerY] == true || HasPlayer2[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        }//end if
                    } else {//IF PLAYER 1 Y POS MOVES UP (NEGATIVELY)
                        return false;
                    }//end if
                } else {//PLAYER 2 MAN MOVEMENT CHECK
                    if(StoredCoords[1] > StoredCoords[3]) {//PLAYER 2 MAN MUST MOVE NEGATIVELY IN Y POS
                        if(StoredCoords[0] < StoredCoords[2]) {//X POS MOVES RIGHT
                            answerX = StoredCoords[0] + 1;//GET JUMPED PIECE POS X
                            answerY = StoredCoords[1] - 1;//GET JUMPED PIECE POS Y

                            //STORE JUMPED POS
                            JumpedPiece = $"{answerX}, {answerY}";

                            if(HasPlayer2[answerX, answerY] == true || HasPlayer1[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        } else {//X POS MOVES LEFT
                            answerX = StoredCoords[0] - 1;//GET JUMPED PIECE POS X
                            answerY = StoredCoords[1] - 1;//GET JUMPED PIECE POS Y

                            //STORE JUMPED POS
                            JumpedPiece = $"{answerX}, {answerY}";

                            if(HasPlayer2[answerX, answerY] == true || HasPlayer1[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        }//end if
                    } else {
                        return false;
                    }//end if
                }//end if
            } else if (ManOrKing[StoredCoords[0], StoredCoords[1]] == "king") {//CHECKS IF KING (ManOrKing)
                if(StoredCoords[1] < StoredCoords[3]) {//Y POS MOVED DOWN
                    if(StoredCoords[0] < StoredCoords[2]) {//X POS MOVED RIGHT
                        answerX = StoredCoords[0] + 1;//GET JUMPED PIECE POS X
                        answerY = StoredCoords[1] + 1;//GET JUMPED PIECE POS Y

                        //STORE JUMPED POS
                        JumpedPiece = $"{answerX}, {answerY}";

                        if(PlayerTurn == 1) {//IF PLAYER 1
                            if(HasPlayer1[answerX, answerY] == true || HasPlayer2[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        } else {//IF PLAYER 2
                            if(HasPlayer2[answerX, answerY] == true || HasPlayer1[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        }//end if
                    } else {//X POS MOVED LEFT
                        answerX = StoredCoords[0] - 1;
                        answerY = StoredCoords[1] + 1;

                        if(PlayerTurn == 1) {//IF PLAYER 1
                            if(HasPlayer1[answerX, answerY] == true || HasPlayer2[answerX, answerY] != true) {
                                return false;
                            }//end if
                        } else {//IF PLAYER 2
                            if(HasPlayer2[answerX, answerY] == true || HasPlayer1[answerX, answerY] != true) {
                                return false;
                            }//end if 
                        }//end if
                    }//end if
                } else {//Y POS MOVED UP
                    if(StoredCoords[0] < StoredCoords[2]) {//X POS MOVED RIGHT
                        answerX = StoredCoords[0] + 1;//GET JUMPED PIECE POS X
                        answerY = StoredCoords[1] - 1;//GET JUMPED PIECE POS Y

                        //STORE JUMPED POS
                        JumpedPiece = $"{answerX}, {answerY}";

                        if(PlayerTurn == 1) {//IF PLAYER 1
                            if(HasPlayer1[answerX, answerY] == true || HasPlayer2[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        } else {//IF PLAYER 2
                            if(HasPlayer2[answerX, answerY] == true || HasPlayer1[answerX, answerY] != true) {//CAN'T JUMP YOURSELF || CAN'T JUMP NOTHING
                                return false;
                            }//end if
                        }//end if
                    } else {//X POS MOVED LEFT
                        answerX = StoredCoords[0] - 1;
                        answerY = StoredCoords[1] - 1;

                        if(PlayerTurn == 1) {//IF PLAYER 1
                            if(HasPlayer1[answerX, answerY] == true || HasPlayer2[answerX, answerY] != true) {
                                return false;
                            }//end if
                        } else {
                            if(HasPlayer2[answerX, answerY] == true || HasPlayer1[answerX, answerY] != true) {
                                return false;
                            }//end if 
                        }//end if
                    }//end if
                }//end if
            } else {//CHECKS IF NONE (ManOrKing)
                return false;
            }//end if

            DidJump = true; //LETS BOARD KNOW SOMEONE MADE A LEGIT JUMP
            return true;
        }//end function
        #endregion

        //STEP 3
        #region CHECK PIECE IF ACHIEVED KING STATUS
        static void KingStatusFunc() {

            if(PlayerTurn == 1 && StoredCoords[3] == (Size - 1) && ManOrKing[StoredCoords[0], StoredCoords[1]] == "man") {
                //PLAYER 1 && ON FURTHEST PLAYER 2 SIDE && IS "MAN"
                ManOrKing[StoredCoords[0], StoredCoords[1]] = "king";
            } else if (PlayerTurn == 2 && StoredCoords[3] == 0 && ManOrKing[StoredCoords[0], StoredCoords[1]] == "man") {
                //PLAYER 2 && ON FURTHEST PLAYER 1 SIDE && IS "MAN"
                ManOrKing[StoredCoords[0], StoredCoords[1]] = "king";
            }//end if
        }//end function
        #endregion

        //STEP 4
        #region MOVE PIECE FROM (X1, Y1) TO (X2, Y2)
        static void MovePieceFunc() {
            //COMBINE STORED COORDS TO FEED DrawSquareFunc
            string movedfrom = SquareCursorPos[StoredCoords[0], StoredCoords[1]];
            string movedto = SquareCursorPos[StoredCoords[2], StoredCoords[3]];

            //OVERWRITE THE ORIGINAL BLOCK (WHERE PLAYER WAS)
            DrawSquareFunc(movedfrom, BoardColor2);

            if(ManOrKing[StoredCoords[0], StoredCoords[1]] == "man") {
                if(PlayerTurn == 1) {
                    //PLACE MAN DOWN IN NEW SLOT
                    DrawCheckerFunc(movedto, Player1Color);
                    HasPlayer1[StoredCoords[0], StoredCoords[1]] = false;   //INFO CHANGE FOR MAN UPDATE
                    HasPlayer1[StoredCoords[2], StoredCoords[3]] = true;    //...
                    ManOrKing[StoredCoords[0], StoredCoords[1]] = "none";   //...
                    ManOrKing[StoredCoords[2], StoredCoords[3]] = "man";    //...
                } else {//PLAYER 2 MOVE
                    DrawCheckerFunc(movedto, Player2Color);
                    HasPlayer2[StoredCoords[0], StoredCoords[1]] = false;   //INFO CHANGE FOR MAN UPDATE
                    HasPlayer2[StoredCoords[2], StoredCoords[3]] = true;    //...
                    ManOrKing[StoredCoords[0], StoredCoords[1]] = "none";   //...
                    ManOrKing[StoredCoords[2], StoredCoords[3]] = "man";    //...
                }//end if
            } else if (ManOrKing[StoredCoords[0], StoredCoords[1]] == "king") {
                if(PlayerTurn == 1) {
                    //PLACE KING DOWN IN NEW SLOT
                    DrawCheckerFunc(movedto, Player1Color);
                    HasPlayer1[StoredCoords[0], StoredCoords[1]] = false;   //INFO CHANGE FOR KING UPDATE
                    HasPlayer1[StoredCoords[2], StoredCoords[3]] = true;    //...
                    ManOrKing[StoredCoords[0], StoredCoords[1]] = "none";   //...
                    ManOrKing[StoredCoords[2], StoredCoords[3]] = "king";   //...
                } else {
                    //PLACE KING DOWN IN NEW SLOT
                    DrawCheckerFunc(movedto, Player2Color);
                    HasPlayer1[StoredCoords[0], StoredCoords[1]] = false;   //INFO CHANGE FOR KING UPDATE
                    HasPlayer1[StoredCoords[2], StoredCoords[3]] = true;    //...
                    ManOrKing[StoredCoords[0], StoredCoords[1]] = "none";   //...
                    ManOrKing[StoredCoords[2], StoredCoords[3]] = "king";   //...
                }//end if
            }//end if
        }//end function
        #endregion

        #region JUMPED PIECE GETS REMOVED
        static void JumpPieceRemovalFunc() {
            ConvertBoardSlot(JumpedPiece);

            //OBTAINED JUMP INFORMATION TO SEND AND GET COORDS
            string sendjumped = $"{SquareCursorPos[BoardStoredCoords[0], BoardStoredCoords[1]]}";

            //DRAW THE SQUARE OVER DEAD PIECE
            DrawSquareFunc(sendjumped, BoardColor2);

            //SETUP FOR CHANGING INFO
            string[] JumpedPieceString = JumpedPiece.Split(',');
            int[] jumpedguy = new int[2];

            for(int index = 0; index < 2; index++) {//LOOP TO STORE COORD DATA OF JUMPED PLAYER
                jumpedguy[index] = int.Parse(JumpedPieceString[index]);
            }//end for

            if(PlayerTurn == 1) {//THEN PLAYER 2 MAN GOT JUMPED
                Player2DeadMen++;//UP PLAYER 2 DEATH COUNT
                HasPlayer2[jumpedguy[0], jumpedguy[1]] = false;//SET AS NO LONGER HAS PLAYER 2
                ManOrKing[jumpedguy[0], jumpedguy[1]] = "none";//...
            } else {
                Player1DeadMen++;//UP PLAYER 1 DEATH COUNT
                HasPlayer1[jumpedguy[0], jumpedguy[1]] = false;//SET AS NO LONGER HAS PLAYER 1
                ManOrKing[jumpedguy[0], jumpedguy[1]] = "none";//...
            }//end if
        }//end region
        #endregion

        //STEP 5
        #region DETERMINED WINNER FUNCTION
        static bool DetermineWinnerFunc() {
            //CHECK IF PLAYER HAS 12 (OR MORE) DEAD PLAYERS
            if(Player2DeadMen >= 12) {//THEN PLAYER 2 LOST
                SetupTextFunc();
                Console.Write($"Player 1 Has Won The Game.");  
                return true;
            } else if (Player1DeadMen >= 12) {//THEN PLAYER 1 LOST
                SetupTextFunc();
                Console.Write($"Player 2 Has Won The Game.");
                return true;
            }//end if

            return false;//NEITHER LOST YET
        }//end function
        #endregion

        //STEP 6
        #region GET PLAYER NUMBER CHANGE
        static int PlayerWhatFunction(int whonow) {
            //FLIP FLOP FOR 1 TO 2, AND 2 TO 1
            if(whonow == 1) {
                whonow++; //CHANGE TO PLAYER 2
            } else if(whonow == 2) {
                whonow--; //CHANGE TO PLAYER 1
            }//end if

            return whonow;
        }//end function
        #endregion

        //PLAY AGAIN?
        #region PLAY AGAIN FUNCTION
        static bool PlayAgainFunc() {
            //VARIABLES
            bool YorN = false;
            char answer = ' ';
            SetupTextFunc();

            
            do {//THIS
                Console.Write("Would you like to play again? (press Y or N to continue)");
                answer = Convert.ToChar(Console.ReadKey());

                if(answer != 'y' && answer != 'n') {//CHECK IF 'Y' OR 'N'
                    SetupTextFunc();
                    Console.Write("Sorry, that's not a real answer. ");//SEND ERROR IF NEITHER 'Y' NOR 'N'
                }//end if
            } while(answer != 'y' && answer != 'n');//LOOP UNTIL GIVEN 'Y' OR 'N' KEYPRESS

            if(answer == 'y') {//THEN SEND TRUE
                YorN = true;
            } else if (answer == 'n') {//THEN SEND FALSE
                YorN = false;
            }//end if

            return YorN;
        }//end function
        #endregion

        //HELPER FUNCTIONS
        #region CONVERT POSITIONS FUNCTION
        static void ConvertPositions(string allcoords) {
            //TAKE INPUT AS (X1Y1-X2Y2) AND...
            char[] charcoords = allcoords.ToCharArray();
            int count = 0;
            //...CONVERT TO ARRAY OF X1, Y1, X2, Y2 AND REMOVE THE HYPHEN
            for(int index = 0; index < charcoords.Length; index++) {
                if(charcoords[index] != '-') {
                    StoredCoords[count] = (Convert.ToInt32(charcoords[index]) - 48);
                    count++;
                }//end if
            }//end for
        }//end function
        #endregion

        #region CONVERT FOR BOARD SLOT
        static void ConvertBoardSlot(string allcoords) {
            //SPLIT ON COMMA TO GET X,Y COORDS
            string[] coords = allcoords.Split(',');

            //LOOP TO STORE X & Y COORDS FOR CALLING LATER
            for(int index = 0; index < 2; index++) {
                BoardStoredCoords[index] = Convert.ToInt32(coords[index]);
            }
        }//end function
        #endregion

        #region SETUP TEXT FUNCTION
        static void SetupTextFunc() {
            //SETUP TO LOOP BLACK ON BLACK TO WIPE ORIGINAL TEXT DATA
            Console.SetCursorPosition(TextPosX, TextPosY);
            Console.ForegroundColor = OG_BG_Color;
            Console.BackgroundColor = OG_BG_Color;
            for(int index = 0; index < 200; index++) {
                Console.Write(Block);
            }//end for

            if(PlayerTurn == 1) {//PLAYER 1 HAS BLACK BACKGROUND WITH WHITE TEXT
                Console.BackgroundColor = Player1Color;
                Console.ForegroundColor = Player2Color;
                Console.SetCursorPosition(TextPosX, TextPosY);
            } else if (PlayerTurn == 2) {//PLAYER 2 HAS WHITE BACKGROUND WITH BLACK TEXT
                Console.BackgroundColor = Player2Color;
                Console.ForegroundColor = Player1Color;
                Console.SetCursorPosition(TextPosX, TextPosY);
            }//end if
            
        }//end function
        #endregion

        //PREBUILT FUNCTION
        #region PREBUILT FUNCTIONS
        static int PromptInt(string message) {//prompts for an int
            Console.Write(message + " ");
            return int.Parse(Console.ReadLine());
        }//end function

        static double PromptDouble(string message) {//prompts for a double
            Console.Write(message + " ");
            return double.Parse(Console.ReadLine());
        }//end function

        static string PromptString(string message) {//prompts for a string
            Console.Write(message + " ");
            return Console.ReadLine();
        }//end funciton

        static bool PromptBool(string message) {//prompts for a bool (may need fixing and/or changing of commands to work)
            Console.Write(message + " ");
            return bool.Parse(Console.ReadLine());
        }//end function

        static char PromptChar(string message) {//prompts for a char
            string check;

            //send message for char and loop if length ! = 1
            do {
                Console.Write(message + " ");
                check = Console.ReadLine();
            } while(check.Length != 1);
            return Convert.ToChar(check);//will only read first input
        }//end function

        static bool StringContains(string word, char letter) {//checks if word contains letter
            bool contains = false;

            //check if word[] contains letter
            for(int index = 0; index < word.Length; index++) {
                if(word[index] == letter) {//then 
                    contains = true;
                }//end if
            }//end for

            return contains;
        }//end function

        static string PadLeft(string word, char letter, int num) {
            string new_letter = Convert.ToString(letter);
            string new_word = "";

            //loop to make letter added to itself the amount in num
            for(int index = 0; index < num; index++) {
                new_word += new_letter;
            }//end for

            //add first word to padded word
            new_word = new_word + word;

            return new_word;
        }//end function

        static string StringRemove(string word, char letter) {
            string new_word = "";

            //loop to find if word[num] does not equal letter
            for(int index = 0; index < word.Length; index++) {
                if(word[index] != letter) {//then add to new_word
                    new_word += word[index];
                }//end if
            }//end for
            return new_word;
        }//end function

        static string StringIntersection(string s1, string s2) {
            string return_string = "";

            //LOOP TO CHECK INTERSECTIONS AND UNIQUE LETTERS
            foreach(char letter in s1) {
                if(in_string(letter, s2) && !in_string(letter, return_string)) {
                    return_string += letter;
                }//end if
            }//end foreach

            //LOCAL HELPER FUNCTION
            bool in_string(char character, string word) {
                foreach(char letter in word) {
                    if(letter == character) {
                        return true;
                    }//end if
                }//end foreach
                return false;
            }//end internal function

            return return_string;
        }//end function

        static string StringUnion(string s1, string s2) {
            string return_string = "";

            //LOOP TO CHECK UNIQUE LETTERS AGAINST s1 (it's self) and return_string
            foreach(char letter in s1) {
                if(in_string(letter, s1) && !in_string(letter, return_string)) {
                    return_string += letter;
                }//end if
            }//end foreach

            //LOOP TO CHECK UNIQUE LETTERS AGAINST s1 AND return_string
            foreach(char letter in s2) {
                if(!in_string(letter, s1) && !in_string(letter, return_string)) {
                    return_string += letter;
                }//end if
            }//end foreach

            //LOCAL HELPER FUNCTION
            bool in_string(char character, string word) {
                foreach(char letter in word) {
                    if(letter == character) {
                        return true;
                    }//end if
                }//end foreach
                return false;
            }//end internal function

            return return_string;
        }//end function

        static bool StringIsNumeric(string input) {
            //loop through each letter and check if < 48 or > 57 (ascii table)
            foreach(char letter in input) {
                if(letter < 48 || letter > 57) {
                    if(letter != 45) {//if not - then
                        return false;
                    }//end if                        
                }//end if                
            }//end foreach
            return true;
        }//end funcion

        static bool StringContains(string s1, string s2) {
            string check_string = "";

            //LOOP TO CHECK LETTERS
            foreach(char letter in s1) {
                if(in_string(letter, s2)) {
                    check_string += letter;
                }//end if
            }//end foreach

            //if check_string matches s2
            if(check_string == s2) {//then
                return true;
            } else {//then
                return false;
            }//end if


            //LOCAL HELPER FUNCTION
            bool in_string(char character, string word) {
                foreach(char letter in word) {
                    if(letter == character) {
                        return true;
                    }//end if
                }//end foreach
                return false;
            }//end internal function
        }//end function

        static string[] StringSplit(string s1, char c1) {
            string word_stored = "";
            int count = 1;

            foreach(char letter in s1) {
                if(c1 == letter) {
                    count++;
                }//end if                
            }//end foreach            

            string[] string_array = new string[count];

            //reset count
            count = -1;

            foreach(char letter in s1) {
                if(in_string(letter, s1)) {
                    word_stored += letter;
                } else {
                    count++;
                    string_array[count] = word_stored;
                    word_stored = "";
                }//end if
            }//end for each

            bool in_string(char character, string word) {
                if(character != c1) {
                    return true;
                }//end if
                return false;
            }//end internal function

            //for final stored string
            if(!s1.EndsWith(c1.ToString())) {
                count++;
                string_array[count] = word_stored;
            }//end if


            return string_array;
        }//end function

        static int[] BubbleSort(int[] data) {
            bool sorting = true;
            int stored_num;
            int count = 0;

            //loop if had to sort
            do {
                //sets everything to start check
                count = 0;
                sorting = false;

                while(count < data.Length && count + 1 != data.Length) {//this loops the entire contents unless it eaches the end
                    if(data[count] > data[count + 1]) {//if [num1] > [num2] store num1 then swap
                        stored_num = data[count];
                        data[count] = data[count + 1];
                        data[count + 1] = stored_num;
                        sorting = true;
                    }//end if
                    count++;
                }//end while    

            } while(sorting);//if had to sort loop again

            return data;
        }//end function

        static int BinarySearch(int[] data, int search) {
            int left = 0;
            int right = data.Length;
            int mid = (left + right) / 2;
            int low_num = data[left];
            int high_num = data[right - 1];

            //call bubble sort to put array in order
            BubbleSort(data);

            //if higher than highest or lower than lowest number skip search
            if(search > high_num || search < low_num) {
                return -1;
            }//end if
            //loop until left > right            
            while(left <= right) {
                if(search == data[mid]) {//then output mid
                    return mid;
                } else if(search < data[mid]) {//then calculate right and change mid accordingly
                    right = mid - 1;
                    mid = (left + right) / 2;
                } else if(search > data[mid]) {//then calculate left and change mid accordingly
                    left = mid + 1;
                    mid = (left + right) / 2;
                }//end for
            }//end while

            //if not found return -1            
            return -1;
        }//end function

        #endregion

    }//end class
}//end namespace
