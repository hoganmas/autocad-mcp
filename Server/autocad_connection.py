import socket
import json
import logging
from dataclasses import dataclass
from typing import Dict, Any
from config import config

# Configure logging using settings from config
logging.basicConfig(
    level=getattr(logging, config.log_level),
    format=config.log_format
)
logger = logging.getLogger("AutoCADMCP")

@dataclass
class AutoCADConnection:
    """Manages the socket connection to the AutoCAD Editor."""
    host: str = config.autocad_host
    port: int = config.autocad_port
    sock: socket.socket = None  # Socket for AutoCAD communication

    def connect(self) -> bool:
        """Establish a connection to the AutoCAD Editor."""
        if self.sock:
            return True
        try:
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.sock.connect((self.host, self.port))
            logger.info(f"Connected to AutoCAD at {self.host}:{self.port}")
            return True
        except Exception as e:
            logger.error(f"Failed to connect to AutoCAD: {str(e)}")
            self.sock = None
            return False

    def disconnect(self):
        """Close the connection to the AutoCAD Editor."""
        if self.sock:
            try:
                self.sock.close()
            except Exception as e:
                logger.error(f"Error disconnecting from AutoCAD: {str(e)}")
            finally:
                self.sock = None

    def receive_full_response(self, sock, buffer_size=config.buffer_size) -> bytes:
        """Receive a complete response from AutoCAD, handling chunked data."""
        chunks = []
        sock.settimeout(config.connection_timeout)  # Use timeout from config
        try:
            while True:
                chunk = sock.recv(buffer_size)
                if not chunk:
                    if not chunks:
                        raise Exception("Connection closed before receiving data")
                    break
                chunks.append(chunk)
                
                # Process the data received so far
                data = b''.join(chunks)
                decoded_data = data.decode('utf-8')
                
                # Check if we've received a complete response
                try:
                    # Special case for ping-pong
                    if decoded_data.strip().startswith('{"status":"success","result":{"message":"pong"'):
                        logger.debug("Received ping response")
                        return data
                    
                    # Handle escaped quotes in the content
                    if '"content":' in decoded_data:
                        # Find the content field and its value
                        content_start = decoded_data.find('"content":') + 9
                        content_end = decoded_data.rfind('"', content_start)
                        if content_end > content_start:
                            # Replace escaped quotes in content with regular quotes
                            content = decoded_data[content_start:content_end]
                            content = content.replace('\\"', '"')
                            decoded_data = decoded_data[:content_start] + content + decoded_data[content_end:]
                    
                    # Validate JSON format
                    json.loads(decoded_data)
                    
                    # If we get here, we have valid JSON
                    logger.info(f"Received complete response ({len(data)} bytes)")
                    return data
                except json.JSONDecodeError:
                    # We haven't received a complete valid JSON response yet
                    continue
                except Exception as e:
                    logger.warning(f"Error processing response chunk: {str(e)}")
                    # Continue reading more chunks as this might not be the complete response
                    continue
        except socket.timeout:
            logger.warning("Socket timeout during receive")
            raise Exception("Timeout receiving AutoCAD response")
        except Exception as e:
            logger.error(f"Error during receive: {str(e)}")
            raise

    def send_command(self, command_type: str, params: Dict[str, Any] = None) -> Dict[str, Any]:
        """Send a command to AutoCAD and return its response."""
        if not self.sock and not self.connect():
            raise ConnectionError("Not connected to AutoCAD")
        
        # Special handling for ping command
        if command_type == "ping":
            try:
                logger.debug("Sending ping to verify connection")
                self.sock.sendall(b"ping")
                response_data = self.receive_full_response(self.sock)
                response = json.loads(response_data.decode('utf-8'))
                
                if response.get("status") != "success":
                    logger.warning("Ping response was not successful")
                    self.sock = None
                    raise ConnectionError("Connection verification failed")
                    
                return {"message": "pong"}
            except Exception as e:
                logger.error(f"Ping error: {str(e)}")
                self.sock = None
                raise ConnectionError(f"Connection verification failed: {str(e)}")
        
        # Normal command handling
        command = {"Type": command_type, "Parameters": params or {}}
        try:
            logger.info(f"Sending command: {command_type} with parameters: {params}")
            self.sock.sendall(json.dumps(command).encode('utf-8'))
            response_data = self.receive_full_response(self.sock)
            response = json.loads(response_data.decode('utf-8'))
            
            if response.get("status") == "error":
                error_message = response.get("error") or response.get("message", "Unknown AutoCAD error")
                logger.error(f"AutoCAD error: {error_message}")
                raise Exception(error_message)
            
            return response.get("result", {})
        except Exception as e:
            logger.error(f"Communication error with AutoCAD: {str(e)}")
            self.sock = None
            raise Exception(f"Failed to communicate with AutoCAD: {str(e)}")

# Global AutoCAD connection
_autocad_connection = None

def get_autocad_connection() -> AutoCADConnection:
    """Retrieve or establish a persistent AutoCAD connection."""
    global _autocad_connection
    if _autocad_connection is not None:
        try:
            # Try to ping with a short timeout to verify connection
            result = _autocad_connection.send_command("ping")
            # If we get here, the connection is still valid
            logger.debug("Reusing existing AutoCAD connection")
            return _autocad_connection
        except Exception as e:
            logger.warning(f"Existing connection failed: {str(e)}")
            try:
                _autocad_connection.disconnect()
            except:
                pass
            _autocad_connection = None
    
    # Create a new connection
    logger.info("Creating new AutoCAD connection")
    _autocad_connection = AutoCADConnection()
    if not _autocad_connection.connect():
        _autocad_connection = None
        raise ConnectionError("Could not connect to AutoCAD. Ensure the AutoCAD Editor and MCP Bridge are running.")
    
    try:
        # Verify the new connection works
        _autocad_connection.send_command("ping")
        logger.info("Successfully established new AutoCAD connection")
        return _autocad_connection
    except Exception as e:
        logger.error(f"Could not verify new connection: {str(e)}")
        try:
            _autocad_connection.disconnect()
        except:
            pass
        _autocad_connection = None
        raise ConnectionError(f"Could not establish valid AutoCAD connection: {str(e)}") 