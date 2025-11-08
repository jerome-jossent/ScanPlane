#pragma region VARIABLES HARDWARE Pins
//define Pulse pin
//define Direction pin
//define Enable Pin
int PUL_X = 13;  //marron
int DIR_X = 12;  //orange
int ENA_X = 11;  //bleu

int ENA_Y = 10;  //orange
int DIR_Y = 9;   //jaune
int PUL_Y = 8;   //bleu

int ENA_Z = 7;  //violet
int DIR_Z = 6;  //gris
int PUL_Z = 5;  //noir
#pragma endregion

bool origine_initialized = false;
float x = 0;
float y = 0;
float x_max = -1;
float y_max = -1;

long delaisPasX_microsec = 100;  // => inverse de vitesse X 100
long delaisPasY_microsec = 100;   // => inverse de vitesse Y 10-5
long delaisPasZ_microsec = 200;  // => inverse de vitesse Z 200

float conversion_X_imp_to_mm = -160.0f; //(32*200)/40 : 32micropas * 200pas <=> 1 tour <=> 20 dents <=> 20x2 mm = 40mm
float conversion_Y_imp_to_mm = -160.0f;  //(32*200)/40 : 32micropas * 200pas <=> 1 tour <=> 20 dents <=> 20x2 mm = 40mm
float conversion_Z_imp_to_turn = -6400.0f;

#pragma region DEBUG
bool debug = true;
//exemple : d1 pour activer debug, pour désactiver : d ou d0 ou dcequonveut
void SetDebug(String m) {
  int val = m.substring(1).toInt();
  debug = (val == 1);
}
#pragma endregion

#pragma region VARIABLES Serial exchange
const unsigned int MAX_MESSAGE_LENGTH = 40;
static char message[MAX_MESSAGE_LENGTH];  //buffer lettres message entrant
#pragma endregion

void setup() {
  Serial.begin(9600);

  pinMode(PUL_X, OUTPUT);
  pinMode(DIR_X, OUTPUT);
  pinMode(ENA_X, OUTPUT);

  pinMode(PUL_Y, OUTPUT);
  pinMode(DIR_Y, OUTPUT);
  pinMode(ENA_Y, OUTPUT);

  pinMode(PUL_Z, OUTPUT);
  pinMode(DIR_Z, OUTPUT);
  pinMode(ENA_Z, OUTPUT);

  //fin de course
  pinMode(A0, INPUT_PULLUP);
  pinMode(A1, INPUT_PULLUP);
  pinMode(A2, INPUT_PULLUP);
  pinMode(A3, INPUT_PULLUP);

  Disable_All();

  Move_Z(0.1f);
  Move_Z(-0.1f);

  //  while(1)
  //    TestZ();
  Serial.println("Plane Scanner ready !");
}

#pragma region Fin de course
#define X_min !digitalRead(A0)
#define X_max !digitalRead(A1)
#define Y_min digitalRead(A2)
#define Y_max digitalRead(A3)
#pragma endregion

#pragma region Enable
#define enable_on LOW
#define enable_off HIGH

void Enable_X() {
  digitalWrite(ENA_Y, enable_off);
  digitalWrite(ENA_Z, enable_off);
  digitalWrite(ENA_X, enable_on);
}
void Enable_Y() {
  digitalWrite(ENA_X, enable_off);
  digitalWrite(ENA_Z, enable_off);
  digitalWrite(ENA_Y, enable_on);
}
void Enable_Z() {
  digitalWrite(ENA_X, enable_off);
  digitalWrite(ENA_Y, enable_off);
  digitalWrite(ENA_Z, enable_on);
}
void Disable_All() {
  digitalWrite(ENA_X, enable_off);
  digitalWrite(ENA_Y, enable_off);
  digitalWrite(ENA_Z, enable_off);
}
#pragma endregion

#pragma region Direction
#define direction_X_plus digitalWrite(DIR_X, HIGH)
#define direction_X_moins digitalWrite(DIR_X, LOW)
#define direction_Y_plus digitalWrite(DIR_Y, LOW)
#define direction_Y_moins digitalWrite(DIR_Y, HIGH)
#define direction_Z_plus digitalWrite(DIR_Z, LOW)
#define direction_Z_moins digitalWrite(DIR_Z, HIGH)
#pragma endregion


#pragma region Move
float Move_X(float mm) {
  Enable_X();
  long nbr_imp = mm * conversion_X_imp_to_mm;
  bool direction_positive = nbr_imp > 0;

  if (conversion_X_imp_to_mm < 0)
    direction_positive = !direction_positive;

  //direction
  if (direction_positive) {
    if (debug) Serial.print("X+");
    direction_X_plus;
  } else {
    if (debug) Serial.print("X-");
    direction_X_moins;
  }

  if (nbr_imp < 0)
    nbr_imp = -nbr_imp;

  if (debug) {
    Serial.print("\t");
    Serial.print(nbr_imp);
    Serial.println("imps");
  }

  // if (conversion_X_imp_to_mm < 0)
  //   direction_positive = !direction_positive;

  //distance
  for (long i = 0; i < nbr_imp; i++) {
    if (direction_positive && X_max) {
      Serial.println("X max atteint !");
      Disable_All();
      float d_restant = (float)(nbr_imp - i) / conversion_X_imp_to_mm;
      if (d_restant < 0) d_restant = -d_restant;
      x = x + mm - d_restant;
      return d_restant;
    }
    if (!direction_positive && X_min) {
      Serial.println("X min atteint !");
      Disable_All();
      float d_restant = (float)(nbr_imp - i) / conversion_X_imp_to_mm;
      if (d_restant < 0) d_restant = -d_restant;
      x = x + mm - d_restant;
      return d_restant;
    }
    digitalWrite(PUL_X, HIGH);
    delayMicroseconds(delaisPasX_microsec);
    digitalWrite(PUL_X, LOW);
    delayMicroseconds(delaisPasX_microsec);
  }
  Disable_All();
  x = x + mm;
  return 0;
}

float Move_Y(float mm) {
  Enable_Y();
  long nbr_imp = mm * conversion_Y_imp_to_mm;
  bool direction_positive = nbr_imp > 0;

  //direction
  if (direction_positive) {
    if (debug) Serial.print("Y+");
    direction_Y_plus;
  } else {
    if (debug) Serial.print("Y-");
    direction_Y_moins;
    nbr_imp = -nbr_imp;
  }

  if (debug) {
    Serial.print("\t");
    Serial.print(nbr_imp);
    Serial.println("imps");
  }

  if (conversion_Y_imp_to_mm < 0)
    direction_positive = !direction_positive;

  //distance
  for (long i = 0; i < nbr_imp; i++) {
    if (direction_positive && Y_max) {
      Serial.println("Y max atteint !");
      Disable_All();
      float d_restant = (float)(nbr_imp - i) / conversion_Y_imp_to_mm;
      if (d_restant < 0) d_restant = -d_restant;
      y = y + mm - d_restant;
      return d_restant;
    }
    if (!direction_positive && Y_min) {
      Serial.println("Y min atteint !");
      Disable_All();
      float d_restant = (float)(nbr_imp - i) / conversion_Y_imp_to_mm;
      if (d_restant < 0) d_restant = -d_restant;
      y = y + mm - d_restant;
      return d_restant;
    }
    digitalWrite(PUL_Y, HIGH);
    delayMicroseconds(delaisPasY_microsec);
    digitalWrite(PUL_Y, LOW);
    delayMicroseconds(delaisPasY_microsec);
  }
  Disable_All();
  y = y + mm;
  return 0;
}

float Move_Z(float tour) {
  Enable_Z();
  long nbr_imp = tour * conversion_Z_imp_to_turn;
  bool direction_positive = nbr_imp > 0;

  //direction
  if (direction_positive) {
    if (debug) Serial.print("Z+");
    direction_Z_plus;
  } else {
    if (debug) Serial.print("Z-");
    direction_Z_moins;
    nbr_imp = -nbr_imp;
  }

  if (debug) {
    Serial.print("\t");
    Serial.print(nbr_imp);
    Serial.println("imps");
  }

  //distance
  for (long i = 0; i < nbr_imp; i++) {
    digitalWrite(PUL_Z, HIGH);
    delayMicroseconds(delaisPasZ_microsec);
    digitalWrite(PUL_Z, LOW);
    delayMicroseconds(delaisPasZ_microsec);
  }
  Disable_All();
  return 0;
}
#pragma endregion


void loop() {
  //================Communication Management===================
  if (!SerialManager()) {  //en attente d'une commande
    delay(1);
    return;
  }

  //================Interaction Management=================
  if (!MessageManager()) {  //en attente d'une commande gérable
    delay(1);
    return;
  }
}

bool SerialManager() {
  if (Serial.available() == 0)
    return false;
  bool messageComplete = false;

  while (Serial.available() > 0) {
    static unsigned int message_pos = 0;

    // Read the next available byte in the serial receive buffer
    char inByte = Serial.read();
    messageComplete = (inByte == '\n');
    // Message coming in (check not terminating character) and guard for over message size
    if (!messageComplete && (message_pos < MAX_MESSAGE_LENGTH - 1)) {
      // Add the incoming byte to our message
      message[message_pos] = inByte;
      message_pos++;
    } else {  // Full message received...
      // Add null character to string
      message[message_pos] = '\0';
      // Reset for the next message
      message_pos = 0;
    }
  }
  return messageComplete;
}

bool MessageManager() {
  switch (message[0]) {

    case '1':
      GotoSO();
      break;
    case '3':
      GotoSE();
      break;
    case '7':
      GotoNO();
      break;
    case '9':
      GotoNE();
      break;

    case 'c':
      GotoMax();
      GotoMin();
      break;

    case 'd':  //debug
      SetDebug(message);
      break;

    case 'f':
      SendFinDeCourse();
      break;

    case 'g':
      GoTo(message);
      SendPosition();
      break;

    case 'h': // chane Y delay
      SetYDelay(message);
      break;

    case 'j':
      GotoMax();
      break;

    case 'k':
      SendPositionMax();
      break;

    case 'o':
      GotoMin();
      break;

    case 'p':  //send position
      SendPosition();
      break;

    case 'x':  //marche
      SetX(message);
      SendPosition();
      break;

    case 'y':  //marche
      SetY(message);
      SendPosition();
      break;

    case 'z':  //marche
      SetZ(message);
      break;

    default:
      Serial.print("Message non pris en compte : ");
      Serial.println(message);
      return false;
  }
  //message traité avec succès
  return true;
}

void GotoMin(){
  Move_X(-1000);  //195 mm de plage
  SendPosition();
  Move_Y(-1000);  //205 mm de plage
  SendPosition();
  SetOrigine();
  SendPosition();
}
void GotoMax(){
  Move_X(1000);  //195 mm de plage
  SendPosition();
  Move_Y(1000);  //195 mm de plage
  SetMax();
  SendPositionMax();  
  SendPosition();
}

void GotoSO(){
  Move_X(-1000);
  Move_Y(-1000);
  SendPosition();
}
void GotoSE(){
  Move_X(1000);
  Move_Y(-1000);
  SendPosition();
}

void GotoNO(){
  Move_X(-1000);
  Move_Y(1000);
  SendPosition();
}
void GotoNE(){
  Move_X(1000);
  Move_Y(1000);
  SendPosition();
}

void SendPosition() {
  Serial.print("Position = ");
  Serial.print(x);
  Serial.print(", ");
  Serial.println(y);
}
void SendPositionMax() {
  Serial.print("PositionMax = ");
  Serial.print(x_max);
  Serial.print(", ");
  Serial.println(y_max);
}

void SendFinDeCourse() {
  Serial.print("X-+Y-+ : ");
  Serial.print(X_min);
  Serial.print(X_max);
  Serial.print(Y_min);
  Serial.print(Y_max);
  Serial.println("");
}

void SetOrigine() {
  x = 0;
  y = 0;
  origine_initialized = true;
}
void SetMax() {
  x_max = x;
  y_max = y;
}

void SetX(String m) {
  float moveX = m.substring(1).toFloat();
  float d_restant = Move_X(moveX);
  if (d_restant != 0) {
    Serial.print("Move X restant : ");
    Serial.print(d_restant);
    Serial.println("mm");
  }
}

void SetY(String m) {
  float moveY = m.substring(1).toFloat();
  float d_left = Move_Y(moveY);
  if (d_left != 0) {
    Serial.print("Move X restant : ");
    Serial.print(d_left);
    Serial.println("mm");
  }
}

void SetZ(String m) {
  float moveZ = m.substring(1).toFloat();
  Move_Z(moveZ);
}

void SetYDelay(String m){
  int tmp = m.substring(1).toInt();
  delaisPasY_microsec = tmp;
}

void GoTo(String m) {
  if (!origine_initialized) {
    Serial.println("Set origine first, GoTo Aborded.");
    return;
  }
  //g2.40;2.40
  int sepIndex = m.indexOf(';');  // position du point-virgule

  String xStr = m.substring(1, sepIndex);   // à partir de l'indice 1 pour sauter 'g'
  String yStr = m.substring(sepIndex + 1);  // après le ';'

  float newx = xStr.toFloat();
  float newy = yStr.toFloat();

  Move_X(newx - x);
  Move_Y(newy - y);
}


#pragma region TESTS
void TestXYZ() {
  TestX();
  TestY();
  TestZ();
}
void TestX() {
  Move_X(10);
  delay(1000);

  Move_X(-10);
  delay(1000);
}
void TestY() {
  Move_Y(10);
  delay(1000);

  Move_Y(-10);
  delay(1000);
}
void TestZ() {
  Move_Z(1);
  delay(1000);

  Move_Z(-1);
  delay(1000);
}

#pragma endregion