// ===== CONFIGURATION DES PINS =====
// Moteur X
const int ENA_X = 11;  // bleu
const int DIR_X = 12;  // orange
const int PUL_X = 13;  // marron

// Moteur Y
const int ENA_Y = 10;  // orange
const int DIR_Y = 9;   // jaune
const int PUL_Y = 8;   // bleu

// Fins de course
#define X_min !digitalRead(A0)
#define X_max !digitalRead(A1)
#define Y_min digitalRead(A2)
#define Y_max digitalRead(A3)

// ===== PARAMETRES MECANIQUE =====
const float conversion_X_imp_to_mm = 160.0f; // impulsions par mm
const float conversion_Y_imp_to_mm = 160.0f;

// ===== PARAMETRES MOUVEMENT =====
const unsigned long PULSE_WIDTH = 100;       // largeur impulsion µs
const unsigned long MIN_STEP_DELAY = 200;    // délai min entre pas (µs) = vitesse max
const unsigned long MAX_STEP_DELAY = 1000;   // délai max entre pas (µs) = vitesse min
const float ACCEL_FACTOR = 0.98;             // facteur d'accélération (0.9-0.99)

// ===== VARIABLES D'ETAT =====
struct AxisState {
  long position;           // position actuelle en impulsions
  long targetPosition;     // position cible en impulsions
  unsigned long stepDelay; // délai entre pas (µs)
  unsigned long lastStepTime; // dernier pas en µs
  bool moving;             // en mouvement
  int direction;           // direction actuelle
};

AxisState axisX = {0, 0, MAX_STEP_DELAY, 0, false, 0};
AxisState axisY = {0, 0, MAX_STEP_DELAY, 0, false, 0};

// Limites après étalonnage
long X_limit_max = 0;
long Y_limit_max = 0;
bool isCalibrated = false;

// Mode déplacement continu
bool continuousX = false;
bool continuousY = false;
int continuousXDir = 0;  // -1, 0, +1
int continuousYDir = 0;

// ===== BUFFER COMMANDES =====
String inputBuffer = "";

// ===== SETUP =====
void setup() {
  Serial.begin(115200);
  
  // Configuration pins moteur X
  pinMode(ENA_X, OUTPUT);
  pinMode(DIR_X, OUTPUT);
  pinMode(PUL_X, OUTPUT);
  digitalWrite(ENA_X, LOW);  // Activer moteur
  digitalWrite(PUL_X, LOW);
  
  // Configuration pins moteur Y
  pinMode(ENA_Y, OUTPUT);
  pinMode(DIR_Y, OUTPUT);
  pinMode(PUL_Y, OUTPUT);
  digitalWrite(ENA_Y, LOW);  // Activer moteur
  digitalWrite(PUL_Y, LOW);
  
  // Configuration fins de course
  pinMode(A0, INPUT_PULLUP);
  pinMode(A1, INPUT_PULLUP);
  pinMode(A2, INPUT_PULLUP);
  pinMode(A3, INPUT_PULLUP);
  
  Serial.println("CNC 2 axes pret");
  Serial.println("Commandes: CAL, POS x y, CONT axis dir, STOP, GET");
}

// ===== LOOP PRINCIPAL =====
void loop() {
  // Lecture commandes série
  while (Serial.available() > 0) {
    char c = Serial.read();
    if (c == '\n' || c == '\r') {
      if (inputBuffer.length() > 0) {
        processCommand(inputBuffer);
        inputBuffer = "";
      }
    } else {
      inputBuffer += c;
    }
  }
  
  // Gestion mouvement continu
  if (continuousX && continuousXDir != 0) {
    long target = axisX.position + (continuousXDir * 100);
    if (isMoveSafe(target, axisY.position)) {
      axisX.targetPosition = target;
      axisX.moving = true;
    }
  }
  
  if (continuousY && continuousYDir != 0) {
    long target = axisY.position + (continuousYDir * 100);
    if (isMoveSafe(axisX.position, target)) {
      axisY.targetPosition = target;
      axisY.moving = true;
    }
  }
  
  // Mise à jour mouvements - les deux axes indépendamment
  updateMotion();
}

// ===== TRAITEMENT COMMANDES =====
void processCommand(String cmd) {
  cmd.trim();
  cmd.toUpperCase();
  
  if (cmd.startsWith("CAL")) {
    calibrate();
  }
  else if (cmd.startsWith("POS ")) {
    // Format: POS x y
    int firstSpace = cmd.indexOf(' ', 4);
    if (firstSpace > 0) {
      float x = cmd.substring(4, firstSpace).toFloat();
      float y = cmd.substring(firstSpace + 1).toFloat();
      moveTo(x, y);
    }
  }
  else if (cmd.startsWith("CONT ")) {
    // Format: CONT X/Y +1/-1/0
    char axis = cmd.charAt(5);
    int dir = cmd.substring(7).toInt();
    setContinuousMove(axis, dir);
  }
  else if (cmd.startsWith("STOP")) {
    stopAllMotion();
  }
  else if (cmd.startsWith("GET")) {
    sendPosition();
  }
  else {
    Serial.println("ERR: Commande inconnue");
  }
}

// ===== CALIBRATION =====
void calibrate() {
  Serial.println("Debut calibration...");
  
  // Désactiver mode continu
  continuousX = false;
  continuousY = false;
  
  // 1. Aller à l'origine (0,0) - coins min
  Serial.println("Recherche origine (0,0)...");
  homeAxis('X');
  homeAxis('Y');
  
  axisX.position = 0;
  axisY.position = 0;
  Serial.println("Position (0,0) atteinte");
  delay(500);
  
  // 2. Trouver X max
  Serial.println("Recherche X max...");
  moveAxisUntilLimit('X', 1);
  X_limit_max = axisX.position;
  float x_max_mm = (float)X_limit_max / conversion_X_imp_to_mm;
  Serial.print("X max: ");
  Serial.print(x_max_mm);
  Serial.println(" mm");
  delay(500);
  
  // 3. Trouver Y max
  Serial.println("Recherche Y max...");
  moveAxisUntilLimit('Y', 1);
  Y_limit_max = axisY.position;
  float y_max_mm = (float)Y_limit_max / conversion_Y_imp_to_mm;
  Serial.print("Y max: ");
  Serial.print(y_max_mm);
  Serial.println(" mm");
  
  isCalibrated = true;
  Serial.println("Calibration terminee");
  Serial.print("Zone de travail: ");
  Serial.print(x_max_mm);
  Serial.print(" x ");
  Serial.print(y_max_mm);
  Serial.println(" mm");
  
  // Retour à l'origine
  moveTo(0, 0);
}

void homeAxis(char axis) {
  if (axis == 'X') {
    while (!X_min) {
      stepMotor('X', -1);
      delayMicroseconds(PULSE_WIDTH);
    }
  } else {
    while (!Y_min) {
      stepMotor('Y', -1);
      delayMicroseconds(PULSE_WIDTH);
    }
  }
}

void moveAxisUntilLimit(char axis, int direction) {
  if (axis == 'X') {
    while (!X_max) {
      stepMotor('X', direction);
      if (direction > 0) axisX.position++;
      else axisX.position--;
      // delayMicroseconds(PULSE_WIDTH);
    }
  } else {
    while (!Y_max) {
      stepMotor('Y', direction);
      if (direction > 0) axisY.position++;
      else axisY.position--;
      // delayMicroseconds(PULSE_WIDTH);
    }
  }
}

// ===== DEPLACEMENT VERS POSITION =====
void moveTo(float x_mm, float y_mm) {
  if (!isCalibrated) {
    Serial.println("ERR: Calibration requise");
    return;
  }
  
  long targetX = (long)(x_mm * conversion_X_imp_to_mm);
  long targetY = (long)(y_mm * conversion_Y_imp_to_mm);
  
  if (!isMoveSafe(targetX, targetY)) {
    Serial.println("ERR: Position hors limites");
    return;
  }
  
  axisX.targetPosition = targetX;
  axisY.targetPosition = targetY;
  axisX.moving = true;
  axisY.moving = true;
  axisX.stepDelay = MAX_STEP_DELAY;
  axisY.stepDelay = MAX_STEP_DELAY;
  
  Serial.print("Deplacement vers (");
  Serial.print(x_mm);
  Serial.print(", ");
  Serial.print(y_mm);
  Serial.println(")");
}

// ===== DEPLACEMENT CONTINU =====
void setContinuousMove(char axis, int direction) {
  if (axis == 'X') {
    continuousX = (direction != 0);
    continuousXDir = direction;
    if (direction != 0) {
      Serial.print("Mode continu X: ");
      Serial.println(direction > 0 ? "+" : "-");
    } else {
      Serial.println("Arret continu X");
    }
  } else if (axis == 'Y') {
    continuousY = (direction != 0);
    continuousYDir = direction;
    if (direction != 0) {
      Serial.print("Mode continu Y: ");
      Serial.println(direction > 0 ? "+" : "-");
    } else {
      Serial.println("Arret continu Y");
    }
  }
}

// ===== ARRET =====
void stopAllMotion() {
  axisX.targetPosition = axisX.position;
  axisY.targetPosition = axisY.position;
  axisX.moving = false;
  axisY.moving = false;
  axisX.stepDelay = MAX_STEP_DELAY;
  axisY.stepDelay = MAX_STEP_DELAY;
  continuousX = false;
  continuousY = false;
  Serial.println("Arret total");
}

// ===== MISE A JOUR MOUVEMENT =====
void updateMotion() {
  unsigned long currentTime = micros();
  
  // Mise à jour axe X
  if (axisX.moving && axisX.position != axisX.targetPosition) {
    if (currentTime - axisX.lastStepTime >= axisX.stepDelay) {
      long remaining = abs(axisX.targetPosition - axisX.position);
      int direction = (axisX.targetPosition > axisX.position) ? 1 : -1;
      
      // Vérification sécurité
      if ((direction > 0 && X_max) || (direction < 0 && X_min)) {
        axisX.targetPosition = axisX.position;
        axisX.moving = false;
        Serial.println("WARN: Fin de course X atteint");
      } else {
        // Gestion rampe d'accélération/décélération
        // unsigned long decelDistance = 500; // distance de décélération en pas
        
        // if (remaining > decelDistance) {
        //   // Accélération
        //   axisX.stepDelay = (unsigned long)(axisX.stepDelay * ACCEL_FACTOR);
        //   if (axisX.stepDelay < MIN_STEP_DELAY) {
        //     axisX.stepDelay = MIN_STEP_DELAY;
        //   }
        // } else {
        //   // Décélération
        //   axisX.stepDelay = (unsigned long)(axisX.stepDelay / ACCEL_FACTOR);
        //   if (axisX.stepDelay > MAX_STEP_DELAY) {
        //     axisX.stepDelay = MAX_STEP_DELAY;
        //   }
        // }
        axisX.stepDelay = PULSE_WIDTH;

        stepMotor('X', direction);
        axisX.position += direction;
        axisX.lastStepTime = currentTime;
        axisX.direction = direction;
        

//ne faire que X, puis que Y
        return;
      }
    }
  } else {
    axisX.moving = false;
    axisX.stepDelay = MAX_STEP_DELAY;
  }
  
  // Mise à jour axe Y (identique mais indépendant)
  if (axisY.moving && axisY.position != axisY.targetPosition) {
    if (currentTime - axisY.lastStepTime >= axisY.stepDelay) {
      long remaining = abs(axisY.targetPosition - axisY.position);
      int direction = (axisY.targetPosition > axisY.position) ? 1 : -1;
      
      // Vérification sécurité
      if ((direction > 0 && Y_max) || (direction < 0 && Y_min)) {
        axisY.targetPosition = axisY.position;
        axisY.moving = false;
        Serial.println("WARN: Fin de course Y atteint");
      } else {
        // Gestion rampe d'accélération/décélération
        // unsigned long decelDistance = 500; // distance de décélération en pas
        
        // if (remaining > decelDistance) {
        //   // Accélération
        //   axisY.stepDelay = (unsigned long)(axisY.stepDelay * ACCEL_FACTOR);
        //   if (axisY.stepDelay < MIN_STEP_DELAY) {
        //     axisY.stepDelay = MIN_STEP_DELAY;
        //   }
        // } else {
        //   // Décélération
        //   axisY.stepDelay = (unsigned long)(axisY.stepDelay / ACCEL_FACTOR);
        //   if (axisY.stepDelay > MAX_STEP_DELAY) {
        //     axisY.stepDelay = MAX_STEP_DELAY;
        //   }
        // }      
              axisY.stepDelay = PULSE_WIDTH;

        
        stepMotor('Y', direction);
        axisY.position += direction;
        axisY.lastStepTime = currentTime;
        axisY.direction = direction;
      }
    }
  } else {
    axisY.moving = false;
    axisY.stepDelay = MAX_STEP_DELAY;
  }
}

// ===== GENERATION IMPULSION =====
void stepMotor(char axis, int direction) {
  if (axis == 'X') {
    digitalWrite(DIR_X, direction > 0 ? HIGH : LOW);
    delayMicroseconds(PULSE_WIDTH); // Délai après changement de direction
    digitalWrite(PUL_X, HIGH);
    delayMicroseconds(PULSE_WIDTH);
    digitalWrite(PUL_X, LOW);
  } else {
    digitalWrite(DIR_Y, direction > 0 ? HIGH : LOW);
    delayMicroseconds(PULSE_WIDTH); // Délai après changement de direction
    digitalWrite(PUL_Y, HIGH);
    delayMicroseconds(PULSE_WIDTH);
    digitalWrite(PUL_Y, LOW);
  }
}

// ===== VERIFICATION SECURITE =====
bool isMoveSafe(long targetX, long targetY) {
  if (!isCalibrated) return true; // Avant calibration, pas de limites
  
  // Vérification limites
  if (targetX < 0 || targetX > X_limit_max) return false;
  if (targetY < 0 || targetY > Y_limit_max) return false;
  
  return true;
}

// ===== ENVOI POSITION =====
void sendPosition() {
  float x_mm = (float)axisX.position / conversion_X_imp_to_mm;
  float y_mm = (float)axisY.position / conversion_Y_imp_to_mm;
  
  Serial.print("POS ");
  Serial.print(x_mm, 2);
  Serial.print(" ");
  Serial.print(y_mm, 2);
  Serial.print(" ");
  Serial.println(isCalibrated ? "CAL" : "UNCAL");
}